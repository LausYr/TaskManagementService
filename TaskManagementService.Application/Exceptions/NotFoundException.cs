using System;

namespace TaskManagementService.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
            : base($"Ресурс '{name}' с ключом '{key}' не найден.")
        {
        }
    }
}