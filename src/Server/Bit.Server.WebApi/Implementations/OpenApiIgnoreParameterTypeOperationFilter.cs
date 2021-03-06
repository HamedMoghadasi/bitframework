﻿using Swashbuckle.Swagger;
using System;
using System.Linq;
using System.Reflection;
using System.Web.Http.Description;

namespace Bit.WebApi.Implementations
{
    public class OpenApiIgnoreParameterTypeOperationFilter<TParameterType> : IOperationFilter
    {
        public virtual void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (schemaRegistry == null)
                throw new ArgumentNullException(nameof(schemaRegistry));

            if (apiDescription == null)
                throw new ArgumentNullException(nameof(apiDescription));

            Parameter[] excludedParameters = apiDescription.ParameterDescriptions
                .Where(p => p.ParameterDescriptor?.ParameterType != null)
                .Where(p => typeof(TParameterType).GetTypeInfo().IsAssignableFrom(p.ParameterDescriptor.ParameterType.GetTypeInfo()))
                .Select(p => operation.parameters.FirstOrDefault(operationParam => operationParam.name == p.Name))
                .ToArray();

            foreach (Parameter parameter in excludedParameters)
                operation.parameters.Remove(parameter);
        }
    }
}
