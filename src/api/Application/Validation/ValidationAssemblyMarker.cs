namespace Todo.Api.Application.Validation;

/// <summary>
/// Marker for FluentValidation assembly scan (AC-FOUNDATION-008.5).
/// <b>Scope:</b> only validators for <b>inbound HTTP request models</b> (<see cref="TransportValidatorBase{T}"/> + types under <c>Application.Transport</c>).
/// Do not add FluentValidation for domain entities here—domain rules belong in the domain layer; validate in services or dedicated domain validators outside the HTTP pipeline.
/// </summary>
public sealed class ValidationAssemblyMarker
{
    private ValidationAssemblyMarker() { }
}
