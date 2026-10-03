namespace SCMS.Web.Models;

/// <summary>A labelled value for the chart components (category counts, monthly rates, ...).</summary>
public sealed record ChartPoint(string Label, double Value);
