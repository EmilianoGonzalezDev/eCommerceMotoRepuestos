namespace eCommerceMotoRepuestos.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public int? StatusCode { get; set; }
        public string? ErrorMessage { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string GetErrorTitle()
        {
            return StatusCode switch
            {
                400 => "Solicitud Inválida",
                404 => "Página No Encontrada",
                403 => "Acceso Denegado",
                500 => "Error del Servidor",
                503 => "Servicio No Disponible",
                _ => "Error"
            };
        }

        public string GetErrorDescription()
        {
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                return ErrorMessage;
            }

            return StatusCode switch
            {
                400 => "La solicitud no pudo procesarse. Revisa los datos e intenta nuevamente.",
                404 => "Lo sentimos, la página que buscas no existe.",
                403 => "No tienes permiso para acceder a esta página.",
                500 => "Algo salió mal en nuestro servidor. Estamos trabajando en solucionarlo.",
                503 => "El servidor está en mantenimiento. Por favor, intenta más tarde.",
                _ => "Se produjo un error inesperado. Por favor, intenta de nuevo."
            };
        }

        public string GetErrorIcon()
        {
            return StatusCode switch
            {
                400 => "❕",
                404 => "🏍️",
                403 => "🔒",
                500 => "⚙️",
                503 => "🔄",
                _ => "❌"
            };
        }
    }
}
