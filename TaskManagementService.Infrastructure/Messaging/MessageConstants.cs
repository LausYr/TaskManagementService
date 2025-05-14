namespace TaskManagementService.Common
{
    public static class MessageConstants
    {
        public const string TaskCreatedRoutingKey = "task.created";
        public const string TaskUpdatedRoutingKey = "task.updated";
        public const string TaskDeletedRoutingKey = "task.deleted";
        public const string DLQRoutingKey = "dlq.task";
    }
}