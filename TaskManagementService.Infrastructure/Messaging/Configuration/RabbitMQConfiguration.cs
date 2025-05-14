namespace TaskManagementService.Common.Configuration
{
    public class RabbitMQConfiguration
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string ExchangeName { get; set; } = "task_events";
        public string QueueName { get; set; } = "task_events_queue";
        public string DLXExchangeName { get; set; } = "dlx_task_events";
        public string DLQQueueName { get; set; } = "dlq_task_events_queue";
        public string[] RoutingKeys { get; set; } = ["task.created", "task.updated", "task.deleted"];
    }
}