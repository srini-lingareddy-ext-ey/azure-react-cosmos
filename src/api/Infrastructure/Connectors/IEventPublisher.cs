using Todo.Api.Application.EventPublishing;

namespace Todo.Api.Infrastructure.Connectors;

// WO-48: IEventPublisher and NoOpEventPublisher have moved to
// Application.EventPublishing and Infrastructure.EventPublishing respectively.
// This file provides backward-compatible type aliases so existing references compile.
// New code should reference Todo.Api.Application.EventPublishing.IEventPublisher directly.
