using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Api.Controllers;

[ApiController]
[Route("api/v1/classification-rules")]
[Authorize]
[RequireTenantContext]
public sealed class ClassificationRulesController : ControllerBase
{
    private readonly IClassificationRuleRepository _ruleRepo;
    private readonly IClassificationAuditRepository _auditRepo;
    private readonly IRuleDeploymentService _deployService;
    private readonly ICurrentTenantContext _tenantContext;

    public ClassificationRulesController(
        IClassificationRuleRepository ruleRepo,
        IClassificationAuditRepository auditRepo,
        IRuleDeploymentService deployService,
        ICurrentTenantContext tenantContext)
    {
        _ruleRepo = ruleRepo;
        _auditRepo = auditRepo;
        _deployService = deployService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<ClassificationRuleView>>> GetRulesAsync(CancellationToken ct = default)
    {
        var rules = new List<ClassificationRuleView>();
        await foreach (var rule in _ruleRepo.GetAllByTenantAsync(_tenantContext.TenantId, ct))
        {
            rules.Add(new ClassificationRuleView
            {
                RuleId = rule.RuleId,
                Priority = rule.Priority,
                Description = rule.Description,
                Outcome = rule.Outcome.ToString(),
                DeployedAt = rule.DeployedAt,
                Conditions = rule.Conditions.Select(c => new RuleConditionView { Field = c.Field, Operator = c.Operator.ToString(), Value = c.Value }).ToList(),
            });
        }
        return Ok(rules);
    }

    [HttpGet("{ruleId}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetRuleByIdAsync(string ruleId, CancellationToken ct = default)
    {
        var rule = await _ruleRepo.GetByIdAsync(ruleId, _tenantContext.TenantId, ct);
        if (rule is null) return NotFound();
        return Ok(new ClassificationRuleView
        {
            RuleId = rule.RuleId,
            Priority = rule.Priority,
            Description = rule.Description,
            Outcome = rule.Outcome.ToString(),
            DeployedAt = rule.DeployedAt,
            Conditions = rule.Conditions.Select(c => new RuleConditionView { Field = c.Field, Operator = c.Operator.ToString(), Value = c.Value }).ToList(),
        });
    }

    [HttpGet("audit-log")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<ClassificationAuditEntryResponse>>> GetAuditLogAsync(
        [FromQuery] string? outcome, [FromQuery] string? matchedRuleId,
        [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to,
        [FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default)
    {
        var items = new List<ClassificationAuditEntryResponse>();
        await foreach (var entry in _auditRepo.GetByTenantAsync(_tenantContext.TenantId, outcome, matchedRuleId, from, to, limit, offset, ct))
        {
            items.Add(new ClassificationAuditEntryResponse
            {
                Id = entry.Id,
                EventId = entry.EventId,
                MatchedRuleId = entry.MatchedRuleId,
                Outcome = entry.Outcome,
                ClassifiedAt = entry.ClassifiedAt,
                EventType = entry.EventType,
                SourceSystem = entry.SourceSystem,
                MonitorName = entry.MonitorName,
            });
        }
        return Ok(items);
    }

    [HttpPost("reload")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ReloadAsync([FromBody] ReloadRequest request, CancellationToken ct = default)
    {
        foreach (var tenantId in request.TenantIds)
            await _deployService.InvalidateCacheAsync(tenantId, ct);
        return Ok();
    }
}
