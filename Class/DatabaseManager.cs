using MySqlConnector;
using System.Text.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dapper;
using static Microsoft.Maui.ApplicationModel.Permissions;
using SintomApp.Class;
using System.Data;

public class DatabaseManager
{
    private readonly string connectionString = "Server=34.175.242.199;Port=3306;Database=users;User Id=root;Password=@D.Raven20_;";


    //Estados de la credencial
    public enum EstadoCredencial
    {
        UsuarioNoExiste,
        ContrasenaIncorrecta,
        Autenticado,
        Admin,
        Medico,
        ErrorConexion,
        UsuarioDeshabilitado,
        TokenErroneo
    }

    public class TokenRegistroResultado
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? Token { get; set; }
    }

    public static List<int> ObtenerIdsPorPatologia(string? patologia)
    {
        return patologia switch
        {
            "LES" => new List<int> { 1 },
            "Miositis" => new List<int> { 2, 5 },
            "Esclerodermia" => new List<int> { 3, 5 },
            "Vasculitis" => new List<int> { 4 },
            _ => new List<int>()
        };
    }



    //Patologías de los usuarios
    public enum PatologiaUsuario
    {
        NoEncontrada,
        LES,
        Miositis,
        Esclerodermia,
        Vasculitis,
        ErrorConexion
    }

    //Método para obtener usuarios
    public async Task<List<Usuario>> GetUsuariosAsync(int id, int tipo)
    {
        var usuarios = new List<Usuario>();
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();

            string query;

            if (tipo == 1)
            {
                // Admin: obtener todos los usuarios habilitados
                query = "SELECT * FROM usuarios WHERE Enable = @enable";
            }
            else if (tipo == 2)
            {
                // Médico: obtener solo los usuarios asignados a ese médico
                query = "SELECT * FROM usuarios WHERE enable = @enable AND medico = @id";
            }
            else
            {
                throw new ArgumentException("Tipo no válido. Usa 1 para Admin o 2 para Médico.");
            }

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@enable", true);

            if (tipo == 2)
                command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var usuario = new Usuario
                {
                    Id = reader.GetInt32("id"),
                    Nombre = reader.GetString("nombre"),
                    Apellidos = reader.IsDBNull("apellidos") ? null : reader.GetString("apellidos"),
                    Correo = reader.IsDBNull("correo") ? null : reader.GetString("correo"),
                    Contrasena = reader.GetString("contrasena"),
                    Patologia = reader.IsDBNull("patologia") ? null : reader.GetString("patologia"),
                    Admin = reader.GetInt32("admin"),
                    Telefono = reader.IsDBNull("telefono") ? null : reader.GetInt32("telefono"),
                    Enable = reader.IsDBNull("enable") ? null : reader.GetBoolean("enable"),
                    FechaCreacion = reader.IsDBNull("fechacreacion") ? null : reader.GetDateTime("fechacreacion"),
                    Frecuencia = reader.IsDBNull("frecuencia") ? 0 : reader.GetInt32("frecuencia"),
                    PrimerDia = reader.IsDBNull("primerdia") ? null : reader.GetDateTime("primerdia"),
                    Medico = reader.IsDBNull("medico") ? null : reader.GetInt32("medico"),
                    Nacimiento = reader.IsDBNull("nacimiento") ? null : reader.GetDateOnly("nacimiento"),
                    Edad = reader.IsDBNull("edad") ? null : reader.GetInt32("edad")
                };

                usuarios.Add(usuario);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener usuarios: {ex.Message}");
        }
        finally
        {
            connection.Dispose();
        }

        return usuarios;
    }


    //Método para crear nuevo usuario
    public async Task<bool> InsertUsuarioAsync(Usuario usuario)
    {
        int nuevo = await ComprobarUsuario(usuario);
        if (nuevo == -1)
        {
            Console.WriteLine("Error al comprobar el usuario");
            return false; // Error al comprobar usuario
        }
        else if (nuevo > 0)
        {
            Console.WriteLine("Ya existe un usuario con este correo");
            return false; // El usuario ya existe
        }
        else
        {
            using var connection = new MySqlConnection(connectionString);
            try
            {
                await connection.OpenAsync();

                // Crear la consulta SQL con el nuevo campo FechaCreacion
                string query = @"INSERT INTO usuarios 
                                (nombre, apellidos, correo, contrasena, patologia, admin, telefono, enable, fechacreacion, medico, frecuencia, nacimiento, edad)
                                VALUES 
                                (@nombre, @apellidos, @correo, @password, @patologia, @admin, @telefono, @enable, @fechaCreacion, @medicoId, @frecuencia, @nacimiento, @edad)";



                using var command = new MySqlCommand(query, connection);

                // Agregar los parámetros
                command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@apellidos", usuario.Apellidos);
                command.Parameters.AddWithValue("@correo", usuario.Correo);
                command.Parameters.AddWithValue("@password", usuario.Contrasena);
                command.Parameters.AddWithValue("@admin", usuario.Admin);
                command.Parameters.AddWithValue("@telefono", usuario.Telefono);
                command.Parameters.AddWithValue("@enable", usuario.Enable);
                command.Parameters.AddWithValue("@fechaCreacion", DateTime.Now); // o usuario.FechaCreacion si lo tienes
                command.Parameters.AddWithValue("@patologia", usuario.Patologia);
                command.Parameters.AddWithValue("@frecuencia", usuario.Frecuencia);
                command.Parameters.AddWithValue("@medicoId", usuario.Medico);
                command.Parameters.AddWithValue("@nacimiento", usuario.Nacimiento);
                command.Parameters.AddWithValue("@edad", usuario.Edad);

                // Ejecutar
                int result = await command.ExecuteNonQueryAsync();

                long userID = command.LastInsertedId;

                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar usuario: {ex.Message}");
                return false;
            }
            finally
            {
                connection.Dispose();
            }


        }
    }

    //Método para comprobar las credenciales del usuario
    public async Task<DatabaseManager.EstadoCredencial> ComprobarCredenciales(string usuarioOcorreo, string password)
    {
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
            string query = "SELECT contrasena, admin, enable FROM usuarios WHERE (nombre = @usuarioOcorreo OR correo = @usuarioOcorreo)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@usuarioOcorreo", usuarioOcorreo);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return EstadoCredencial.UsuarioNoExiste;
            }

            string contrasenaEnBD = reader.GetString(0);
            int admin = reader.GetInt32(1);
            bool habilitado = reader.GetBoolean(2);

            // Verificar si está habilitado
            if (!habilitado)
            {
                return EstadoCredencial.UsuarioDeshabilitado;
            }

            // Verificar contraseña
            if (contrasenaEnBD != password)
            {
                return EstadoCredencial.ContrasenaIncorrecta;
            }

            // Si llegó hasta aquí, está autenticado
            if (admin == 1)
            {
                return EstadoCredencial.Admin;
            }
            else if (admin == 2)
            {
                return EstadoCredencial.Medico;
            }
            else
            {
                return EstadoCredencial.Autenticado;
            }

        }
        catch (Exception ex)
        {
            return EstadoCredencial.ErrorConexion;
            throw new Exception(ex.Message);
        }
        finally
        {
            connection.Dispose();
        }

    }

    //Método para actualizar el usuario
    public async Task<bool> ActualizarUsuarioAsync(Usuario usuario)
    {
        using var connection = new MySqlConnection(connectionString);
        int nuevo = await ComprobarUsuario(usuario);
        if (nuevo == -1)
        {
            Console.WriteLine("Error al comprobar el usuario");
            return false; // Error al comprobar usuario
        }
        else
        {
            try
            {
                await connection.OpenAsync();
                string query = "UPDATE usuarios SET nombre = @nombre, apellidos = @apellidos, correo = @correo, contrasena = @contrasena, patologia = @patologia, telefono = @telefono, frecuencia = @frecuencia, medico = @medico, nacimiento = @nacimiento, edad = @edad WHERE id = @id";
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@apellidos", usuario.Apellidos);
                command.Parameters.AddWithValue("@correo", usuario.Correo);
                command.Parameters.AddWithValue("@contrasena", usuario.Contrasena);
                command.Parameters.AddWithValue("@patologia", usuario.Patologia);
                command.Parameters.AddWithValue("@telefono", usuario.Telefono);
                command.Parameters.AddWithValue("@admin", usuario.Admin);
                command.Parameters.AddWithValue("@frecuencia", usuario.Frecuencia);
                command.Parameters.AddWithValue("@id", usuario.Id);
                command.Parameters.AddWithValue("@medico", (object?)usuario.Medico ?? DBNull.Value); // Permite que sea nulo
                command.Parameters.AddWithValue("@nacimiento", usuario.Nacimiento);
                command.Parameters.AddWithValue("@edad", usuario.Edad);

                int result = await command.ExecuteNonQueryAsync();
                return result > 0; // Devuelve true si se actualizó al menos una fila
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar usuario: {ex.Message}");
                return false;
            }
            finally
            {
                connection.Dispose();
            }
        }


    }

    //Método para comprobar si el usuario ya existe 
    private async Task<int> ComprobarUsuario(Usuario usuario)
    {
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
            string query = "SELECT COUNT(*) FROM usuarios WHERE correo = @correo";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@correo", usuario.Correo);
            int count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al comprobar usuario: {ex.Message}");
            return -1;
        }
        finally
        {
            connection.Dispose();
        }
    }

    //Método para deshabilitar el usuario
    public async Task<bool> DeshabilitarUsuario(Usuario usuario)
    {
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
            string query = "UPDATE usuarios SET enable = @enable WHERE id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@enable", false);
            command.Parameters.AddWithValue("@id", usuario.Id);
            int result = await command.ExecuteNonQueryAsync();
            return result > 0; // Devuelve true si se actualizó al menos una fila
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al deshabilitar usuario: {ex.Message}");
            return false;
        }
        finally
        {
            connection.Dispose();
        }
    }

    //Método para obtener las preguntas por id
    public async Task<List<Pregunta>> ObtenerPreguntasPorId(Usuario user)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        var idPatologias = ObtenerIdsPorPatologia(user.Patologia);
        var preguntas = new List<Pregunta>();
        if (idPatologias == null || !idPatologias.Any()) return preguntas;

        if (idPatologias.Count == 1 && idPatologias[0] == 4)
        {
            string queryVasculitis = "SELECT id, texto, persistentes, nuevasoempeoran FROM vasculitis";
            try
            {
                using var cmd = new MySqlCommand(queryVasculitis, connection);
                using var rdr = await cmd.ExecuteReaderAsync();

                while (await rdr.ReadAsync())
                {
                    preguntas.Add(new Pregunta
                    {
                        Id = rdr.GetInt32(0),
                        Texto = rdr.GetString(1),
                        PuntuacionPersistente = rdr.GetInt32(2),
                        PuntuacionNueva = rdr.GetInt32(3)
                    });
                }
                return preguntas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener preguntas de vasculitis: {ex.Message}");
                return preguntas;
            }
        }

        try
        {
            var condiciones = idPatologias.Select(id => $"id LIKE '{id}%'");
            var whereClause = string.Join(" OR ", condiciones);
            string query = $"SELECT id, texto, texto2, texto3, tipo, puntuacion FROM preguntas WHERE {whereClause}";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                preguntas.Add(new Pregunta
                {
                    Id = reader.GetInt32(0),
                    Texto = reader.GetString(1),
                    Texto2 = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Texto3 = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Tipo = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Puntuacion = reader.IsDBNull(5) ? null : reader.GetDecimal(5)
                });
            }
            return preguntas;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener preguntas: {ex.Message}");
            return preguntas;
        }
        finally
        {
            connection.Dispose();
        }
    }

    //Método para guardar las respuestas en la base de datos
    public async Task<bool> GuardarRespuestasBD(Dictionary<int, string> respuestas, int idUsuario, decimal? puntuacion)
    {
        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // 🔢 Obtener el número de encuestas previas respondidas por este usuario
            string countQuery = "SELECT COUNT(*) FROM registros WHERE id_usuario = @idUsuario";
            using var countCmd = new MySqlCommand(countQuery, connection);
            countCmd.Parameters.AddWithValue("@idUsuario", idUsuario);
            var result = await countCmd.ExecuteScalarAsync();
            int numeroEncuesta = 1;
            if (result != null && int.TryParse(result.ToString(), out int count))
            {
                numeroEncuesta = count + 1;
            }


            // 🧱 Crear diccionario con columnas a insertar
            var columnas = new Dictionary<string, object>
            {
                { "encuestas", numeroEncuesta },
                { "fecha_respuesta", DateTime.Now },
                { "id_usuario", idUsuario.ToString() },
                { "puntuacion", puntuacion.ToString() },
            };

            foreach (var kvp in respuestas)
            {
                string columna = $"p_{kvp.Key}";
                try
                {
                    JsonDocument.Parse(kvp.Value); // Verifica que sea JSON válido
                    columnas[columna] = kvp.Value;
                }
                catch (JsonException)
                {
                    Console.WriteLine($"Respuesta inválida en pregunta {kvp.Key}: no es un JSON válido.");
                    columnas[columna] = null; // O ignora / lanza excepción
                }
            }

            //Generar dinamicamente la consulta 

            var columnasSql = string.Join(",", columnas.Keys);
            var valoresSql = string.Join(",", columnas.Keys.Select(v => $"@{v}"));
            string query = $"INSERT INTO registros ({columnasSql}) VALUES ({valoresSql})";

            using var cmd = new MySqlCommand(query, connection);

            foreach (var kvp in columnas)
            {
                cmd.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
            }
            await cmd.ExecuteNonQueryAsync();

            if (numeroEncuesta == 1)
            {
                string updateUser = "UPDATE usuarios SET primerdia = @fecha WHERE id = @idUsuario";
                using var updateCmd = new MySqlCommand(updateUser, connection);
                updateCmd.Parameters.AddWithValue("@fecha", DateTime.Now.Date);
                updateCmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                await updateCmd.ExecuteNonQueryAsync();
            }

            return true; // Devuelve true si se insertó al menos una fila
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al guardar respuestas: {ex.Message}");
            return false;
        }
    }

    // En DatabaseManager.cs
    public async Task<List<Medico>> ObtenerMedicosAsync()
    {
        var medicos = new List<Medico>();

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        string query = "SELECT id, nombre, apellidos FROM usuarios WHERE admin = 2";
        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            medicos.Add(new Medico
            {
                Id = reader.GetInt32("id"),
                NombreCompleto = reader.GetString("nombre") + " " + reader.GetString("apellidos")
            });
        }

        return medicos;
    }

    // Obtenemos usuario con credenciales correctas
    public async Task<Usuario> CargarUsuarioBD(string nameOcorreo, string pass)
    {
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
            string query = "SELECT id, nombre, apellidos, correo, contrasena, patologia, telefono, primerdia, frecuencia, admin, medico FROM usuarios WHERE (nombre = @nameOcorreo OR correo = @nameOcorreo) AND contrasena = @pass";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@nameOcorreo", nameOcorreo);
            command.Parameters.AddWithValue("@pass", pass);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Usuario
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Apellidos = reader.GetString(2),
                    Correo = reader.GetString(3),
                    Contrasena = reader.GetString(4),
                    Patologia = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Telefono = reader.GetInt32(6),
                    PrimerDia = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    Frecuencia = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                    Admin = reader.GetInt32(9),
                    Medico = reader.IsDBNull(10) ? null : reader.GetInt32(10)
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar usuario: {ex.Message}");

            return null;
        }
        finally
        {
            connection.Dispose();

        }

    }

    // Comprobar que ha respondido a las encuestas
    public async Task<bool?> EstadoEncuesta(int usuarioId, DateTime fecha)
    {
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();

            string query = @"SELECT COUNT(*)
                             FROM registros 
                             WHERE id_usuario = @usuarioId 
                             AND fecha_respuesta >= @inicioDia
                             AND fecha_respuesta < @finDia";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@usuarioId", usuarioId);
            command.Parameters.AddWithValue("@inicioDia", fecha.Date);
            command.Parameters.AddWithValue("@finDia", fecha.Date.AddDays(1));

            int count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0; // Devuelve true si ya existe una encuesta para esa fecha
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al comprobar estado de encuesta: {ex.Message}");
            return false;
        }
        finally
        {
            connection.Dispose();
        }

    }

    // Todas las encuestas
    public async Task<List<DateTime>> ObtenerFechasEncuestasRealizadas(int usuarioId)
    {
        var fechas = new List<DateTime>();
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();

            string query = @"SELECT DISTINCT DATE(fecha_respuesta) 
                         FROM registros 
                         WHERE id_usuario = @usuarioId";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@usuarioId", usuarioId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                fechas.Add(reader.GetDateTime(0).Date);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener fechas de encuestas: {ex.Message}");
        }
        return fechas;
    }

    // Eventos de los usuarios
    public async Task<List<Evento>> ObtenerEventos(int usuarioId, DateTime fecha)
    {
        var eventos = new List<Evento>();
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
            string query = @"SELECT tipo, descripcion 
                         FROM eventosusuarios
                         WHERE id_usuario = @usuarioId AND fecha = @fecha";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@usuarioId", usuarioId);
            command.Parameters.AddWithValue("@fecha", fecha.Date);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                eventos.Add(new Evento
                {
                    IdUsuario = usuarioId,
                    Fecha = fecha.Date,
                    Tipo = reader.GetString(0),
                    Descripcion = reader.GetString(1)
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo eventos: {ex.Message}");
        }
        return eventos;
    }

    // Eventos del mes
    public async Task<List<Evento>> ObtenerEventosDelMes(int idUsuario, int year, int month)
    {
        var eventos = new List<Evento>();
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
            string query = @"SELECT * FROM eventosusuarios 
                         WHERE id_usuario = @idUsuario 
                         AND YEAR(fecha) = @year AND MONTH(fecha) = @month";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@idUsuario", idUsuario);
            command.Parameters.AddWithValue("@year", year);
            command.Parameters.AddWithValue("@month", month);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                eventos.Add(new Evento
                {
                    Id_evento = reader.GetInt32("id_evento"),
                    IdUsuario = reader.GetInt32("id_usuario"),
                    Fecha = reader.GetDateTime("fecha"),
                    Tipo = reader.GetString("tipo"),
                    Descripcion = reader.GetString("descripcion"),
                    IdGrupo = reader.IsDBNull(reader.GetOrdinal("idgrupo")) ? null : reader.GetString("idgrupo")
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo eventos del mes: {ex.Message}");
        }

        return eventos;
    }

    public async Task<bool> InsertarEvento(Evento evento)
    {
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
            string query = @"INSERT INTO eventosusuarios 
                        (id_usuario, fecha, tipo, descripcion, idgrupo) 
                        VALUES (@idUsuario, @fecha, @tipo, @descripcion, @idGrupo)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@idUsuario", evento.IdUsuario);
            command.Parameters.AddWithValue("@fecha", evento.Fecha);
            command.Parameters.AddWithValue("@tipo", evento.Tipo);
            command.Parameters.AddWithValue("@descripcion", evento.Descripcion);
            command.Parameters.AddWithValue("@idGrupo", (object?)evento.IdGrupo ?? DBNull.Value);

            int filas = await command.ExecuteNonQueryAsync();
            return filas > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error insertando evento: {ex.Message}");
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetUltimoRegistroAsync(int userId)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        string query = "SELECT * FROM registros WHERE idusuario = @id ORDER BY fecha DESC LIMIT 1";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", userId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var respuestas = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var column = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                respuestas[column] = value;
            }

            return respuestas;
        }

        return new Dictionary<string, object>();
    }

    public async Task<Dictionary<Pregunta, string>> ObtenerPreguntasYRespuestasPorFechaAsync(Usuario user, DateTime fechaSeleccionada)
    {
        var resultado = new Dictionary<Pregunta, string>();

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        // 1. Obtener todas las respuestas del usuario en esa fecha
        string query = "SELECT * FROM registros WHERE id_usuario = @id AND DATE(fecha_respuesta) = @fecha";
        using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@id", user.Id);
        cmd.Parameters.AddWithValue("@fecha", fechaSeleccionada.Date);

        var respuestas = new Dictionary<string, string>();
        using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return resultado; // No hay datos para esa fecha

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var col = reader.GetName(i);
            if (col.StartsWith("p_") && !reader.IsDBNull(i))
            {
                respuestas[col] = reader.GetString(i);
            }
        }

        // 2. Asociar cada respuesta con su pregunta correspondiente
        foreach (var entry in respuestas)
        {
            if (!int.TryParse(entry.Key.Substring(2), out int idPregunta))
                continue;

            Pregunta? pregunta = null;
            try
            {
                pregunta = await ObtenerPreguntaPorId(idPregunta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo obtener la pregunta {idPregunta}: {ex.Message}");
                continue;
            }

            string respuesta = entry.Value;

            // Si es tipo JSON (string2 o string3)
            if (respuesta.StartsWith("{"))
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(respuesta);
                    var data = jsonDoc.RootElement;
                    if (data.TryGetProperty("respuesta", out var rpta))
                    {
                        var textoRespuesta = rpta.GetString();
                        if (textoRespuesta == "No")
                        {
                            respuesta = "No";
                        }
                        else if (data.TryGetProperty("detalle", out var detalle))
                        {
                            respuesta = $"Sí - {detalle.GetString()}";
                        }
                        else
                        {
                            respuesta = "Sí";
                        }
                    }
                    else
                    {
                        var partes = data.EnumerateObject().Select(p => $"{p.Name}: {p.Value.GetString()}");
                        respuesta = string.Join(", ", partes);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al parsear JSON de la respuesta de la pregunta {idPregunta}: {ex.Message}");
                    respuesta = "Respuesta no válida";
                }
            }

            if (pregunta != null)
            {
                resultado[pregunta] = respuesta;
            }
        }

        return resultado;
    }

    public async Task<List<(DateTime fecha, decimal puntuacion)>> ObtenerPuntuacionesUsuario(Usuario usuario)
    {
        var resultado = new List<(DateTime, decimal)>();

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        try
        {
            string query = $"SELECT fecha_respuesta, puntuacion FROM registros WHERE id_usuario = @usuarioId ORDER BY fecha_respuesta DESC";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@usuarioId", usuario.Id);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var fecha = reader.GetDateTime("fecha_respuesta");
                decimal puntuacion = reader.GetDecimal("puntuacion");


                resultado.Add((fecha, puntuacion));
            }

            return resultado;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener puntuaciones: {ex.Message}");
            return resultado;
        }


    }

    public async Task<Dictionary<Pregunta, List<(DateTime fecha, string respuesta)>>> ObtenerHistorialPorPregunta(Usuario usuario)
    {
        var resultado = new Dictionary<Pregunta, List<(DateTime, string)>>();

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        string query = $"SELECT * FROM registros WHERE usuario_id = @usuarioId ORDER BY fecha DESC";
        using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@usuarioId", usuario.Id);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var fecha = reader.GetDateTime("fecha");

            foreach (var column in reader.GetColumnSchema().Where(c => c.ColumnName.StartsWith("p_")))
            {
                string rawId = column.ColumnName.Substring(2);
                int preguntaId = int.Parse(rawId);

                // Obtener texto de pregunta (puedes tenerlo cacheado o consultar otra tabla)
                var pregunta = await ObtenerPreguntaPorId(preguntaId);

                string respuesta = reader[column.ColumnName]?.ToString() ?? "";

                if (!resultado.ContainsKey(pregunta))
                    resultado[pregunta] = new List<(DateTime, string)>();

                resultado[pregunta].Add((fecha, respuesta));
            }
        }

        return resultado;
    }

    public async Task<Pregunta> ObtenerPreguntaPorId(int idPregunta)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        string query;
        bool esVasculitis = idPregunta >= 40001 && idPregunta <= 40028;

        try
        {
            if (esVasculitis)
            {
                query = "SELECT id, texto FROM vasculitis WHERE id = @idPregunta";
            }
            else
            {
                query = "SELECT id, texto FROM preguntas WHERE id = @idPregunta";
            }

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@idPregunta", idPregunta);

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new Exception($"No se encontró la pregunta con ID {idPregunta}.");
            }


            return new Pregunta
            {
                Id = reader.GetInt32(0),
                Texto = reader.GetString(1),
            };

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener pregunta con ID {idPregunta}: {ex.Message}");
            throw; // Re-lanza la excepción para que pueda ser capturada por el código que llama
        }
    }

    public async Task<List<RespuestaConFecha>> ObtenerRespuestasPorPregunta(int idUsuario, int idPregunta)
    {
        var respuestas = new List<RespuestaConFecha>();
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();

            string query = "SELECT fecha_respuesta, p_" + idPregunta + " FROM registros WHERE id_usuario = @id ORDER BY fecha_respuesta DESC";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", idUsuario);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var fecha = reader.GetDateTime(0);
                var valor = reader.IsDBNull(1) ? "Sin respuesta" : reader.GetString(1);

                respuestas.Add(new RespuestaConFecha
                {
                    Fecha = fecha,
                    Valor = valor
                });
            }

            return respuestas;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar las respuestas de la pregunta con ID {idPregunta}: {ex.Message}");
            throw; // Re-lanza la excepción para que pueda ser capturada por el código que llama
        }
    }


    public async Task<TokenRegistroResultado> GenerarTokenRegistroAsync(string correo)
    {
        var resultado = new TokenRegistroResultado();

        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            // 1. Comprobar si ya hay un token activo reciente
            string checkQuery = @"
                                    SELECT token 
                                    FROM registro_tokens 
                                    WHERE correo = @correo AND usado = FALSE AND fecha_generacion >= NOW() - INTERVAL 15 DAY
                                    ORDER BY fecha_generacion DESC
                                    LIMIT 1";

            using var checkCommand = new MySqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@correo", correo);
            var tokenExistente = await checkCommand.ExecuteScalarAsync();

            if (tokenExistente != null)
            {
                resultado.Exito = false;
                resultado.Mensaje = "Ya existe un token activo para este correo. Validez: 15 días.";
                resultado.Token = tokenExistente.ToString();
                return resultado;
            }

            // 2. Comprobar si el correo ya está registrado
            string userCheckQuery = "SELECT COUNT(*) FROM usuarios WHERE correo = @correo";
            using var userCheckCommand = new MySqlCommand(userCheckQuery, connection);
            userCheckCommand.Parameters.AddWithValue("@correo", correo);
            var userCount = Convert.ToInt32(await userCheckCommand.ExecuteScalarAsync());

            if (userCount > 0)
            {
                resultado.Exito = false;
                resultado.Mensaje = "El correo ya está registrado como usuario.";
                return resultado;
            }

            // 3. Generar nuevo token
            string nuevoToken = Guid.NewGuid().ToString("N")[..8];
            string insertQuery = "INSERT INTO registro_tokens (correo, token, fecha_generacion) VALUES (@correo, @token, NOW())";

            using var insertCommand = new MySqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@correo", correo);
            insertCommand.Parameters.AddWithValue("@token", nuevoToken);
            await insertCommand.ExecuteNonQueryAsync();

            resultado.Exito = true;
            resultado.Mensaje = "Token generado correctamente.";
            resultado.Token = nuevoToken;
            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Exito = false;
            resultado.Mensaje = $"Error al generar el token: {ex.Message}";
            return resultado;
        }
    }


    public async Task<bool> VerificarTokenRegistroAsync(string correo, string token)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        string query = @"SELECT COUNT(*) FROM registro_tokens 
                     WHERE correo = @correo AND token = @token 
                     AND usado = FALSE AND fecha_generacion >= NOW() - INTERVAL 15 DAY";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@correo", correo);
        command.Parameters.AddWithValue("@token", token);
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task MarcarTokenComoUsadoAsync(string correo, string token)
    {
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        string query = "UPDATE registro_tokens SET usado = TRUE WHERE correo = @correo AND token = @token";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@correo", correo);
        command.Parameters.AddWithValue("@token", token);
        await command.ExecuteNonQueryAsync();
    }

    public async Task GuardarAvisoAsync(int idPaciente, int idMedico, decimal puntuacion, string mensaje)
    {
        using var connection = new MySqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
            string query = "INSERT INTO avisos_medicos (id_paciente, id_medico, mensaje, fecha) VALUES (@idPaciente, @idMedico, @mensaje, NOW())";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@idPaciente", idPaciente);
            command.Parameters.AddWithValue("@idMedico", idMedico);
            command.Parameters.AddWithValue("@mensaje", mensaje);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al guardar aviso: {ex.Message}");
            throw; // Re-lanza la excepción para que pueda ser capturada por el código que llama
        }
        finally
        {
            connection.Dispose();
        }
    }

    public async Task<List<Aviso>> ObtenerAvisosMedicoAsync(int idMedico)
    {
        var avisos = new List<Aviso>();
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        string query = "SELECT id, id_paciente, mensaje, fecha, leido FROM avisos_medicos WHERE id_medico = @idMedico";
        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@idMedico", idMedico);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            avisos.Add(new Aviso
            {
                Id = reader.GetInt32(0),
                IdPaciente = reader.GetInt32(1),
                Mensaje = reader.GetString(2),
                Fecha = reader.GetDateTime(3),
                Leido = reader.GetBoolean(4)
            });
        }
        return avisos;
    }

    public async Task<Usuario?> ObtenerUsuarioPorCorreoAsync(string correo, string token)
    {
        Usuario? usuario = null;
        using var connection = new MySqlConnection(connectionString);
        try
        {

            await connection.OpenAsync();

            string query = "SELECT correo, token, fecha_generacion, usado FROM registro_tokens WHERE correo = @correo AND token = @token";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@correo", correo);
            command.Parameters.AddWithValue("@token", token);


            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return usuario;
            }
            return usuario = new Usuario()
            {
                Correo = correo,
                Token = token,
                Fecha_generacion = reader.GetDateTime(2),
                Usado = reader.GetBoolean(3)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener usuario por correo: {ex.Message}");
            return usuario;
        }
    }

    public async Task<List<decimal>> ObtenerPuntuacionesUsuariosMedico(int idMedico)
    {
        var resultados = new List<decimal>();

        using var conn = new MySqlConnection(connectionString);
        try
        {
            await conn.OpenAsync();

            // 1. Obtener todos los IDs de usuarios asignados a este médico
            List<int> idsUsuarios = new List<int>();
            string queryUsuarios = "SELECT id FROM usuarios WHERE medico = @idMedico";

            using (var cmdUsuarios = new MySqlCommand(queryUsuarios, conn))
            {
                cmdUsuarios.Parameters.AddWithValue("@idMedico", idMedico);
                using var readerUsuarios = await cmdUsuarios.ExecuteReaderAsync();
                while (await readerUsuarios.ReadAsync())
                {
                    idsUsuarios.Add(Convert.ToInt32(readerUsuarios["id"]));
                }
            }

            // Si no hay usuarios, devolver la lista vacía
            if (idsUsuarios.Count == 0)
                return resultados;

            // 2. Buscar puntuaciones en la tabla respuestas para esos usuarios
            // Creamos la consulta dinámica con parámetros
            string placeholders = string.Join(",", idsUsuarios.Select((_, i) => $"@id{i}"));
            string queryPuntuaciones = $"SELECT puntuacion FROM registros WHERE id_usuario IN ({placeholders})";

            using (var cmdPuntuaciones = new MySqlCommand(queryPuntuaciones, conn))
            {
                for (int i = 0; i < idsUsuarios.Count; i++)
                {
                    cmdPuntuaciones.Parameters.AddWithValue($"@id{i}", idsUsuarios[i]);
                }

                using var readerPuntuaciones = await cmdPuntuaciones.ExecuteReaderAsync();
                while (await readerPuntuaciones.ReadAsync())
                {
                    if (readerPuntuaciones["puntuacion"] != DBNull.Value)
                        resultados.Add(Convert.ToDecimal(readerPuntuaciones["puntuacion"]));
                }
            }

            return resultados;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en ObtenerPuntuacionesUsuariosMedico: {ex.Message}");
            return resultados;
        }
    }



}


public class Medico
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; }
}


public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int? Telefono { get; set; }
    public string? Apellidos { get; set; }
    public string Correo { get; set; }
    public string Contrasena { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? PrimerDia { get; set; }
    public int? Frecuencia { get; set; }
    public string? Patologia { get; set; }
    public int Admin { get; set; }
    public int? Medico { get; set; }
    public bool? Enable { get; set; } = true;
    public DateOnly? Nacimiento { get; set; }
    public string? Token { get; set; } // Para el registro
    public bool? Usado { get; set; } // Para el registro
    public DateTime? Fecha_generacion { get; set; } // Para el registro
    public int? Edad { get; set; } // Calculada a partir de la fecha de nacimiento

}

public class Pregunta
{
    public int Id { get; set; }
    public string Texto { get; set; }
    public string? Texto2 { get; set; }
    public string? Texto3 { get; set; }
    public string? Tipo { get; set; }
    public decimal? Puntuacion { get; set; }
    public int? PuntuacionPersistente { get; set; }
    public int? PuntuacionNueva { get; set; }
    public string? PatologiaAsociada { get; set; }

}


public class RespuestaBase
{
    public int Encuestas { get; set; }
    public int IdPregunta { get; set; }
    public int IdUsuario { get; set; }
    public string? Valor1 { get; set; }
    public string? Valor2 { get; set; }
    public string? Valor3 { get; set; }
    public int Puntuacion { get; set; }
    public DateTime Fecha { get; set; }
}

public class ResultadoPatologia
{
    public DatabaseManager.PatologiaUsuario Patologia { get; set; }
    public List<Pregunta> Preguntas { get; set; } = new();

}

public class Evento
{
    public int Id_evento { get; set; }
    public int IdUsuario { get; set; }
    public string? IdGrupo { get; set; }
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; }
    public string Descripcion { get; set; }
}


public class RespuestaConFecha
{
    public DateTime Fecha { get; set; }
    public string Valor { get; set; }
}


public class CuestionarioRespondido
{
    public DateTime Fecha { get; set; }
    public decimal Puntuacion { get; set; }

    public string FechaFormateada => Fecha.ToString("dd/MM/yyyy");
}
public static class SessionManager
{
    public static Usuario UsuarioActual { get; set; }
    public static Usuario UsuarioSeleccionado { get; set; }
    public static Pregunta PreguntaSeleccionada { get; set; }
    public static DateTime FechaSeleccionada { get; set; }
    public static string HtmlRespuestas { get; set; }
    public static bool EsEdicion { get; set; }
    public static string TituloPagina { get; set; }
}
