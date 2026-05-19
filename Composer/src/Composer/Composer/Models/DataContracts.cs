using System.Collections.Immutable;

namespace Composer.Models;

/// <summary>
/// Layer 6 — Data. A list of named entity records, each with explicit fields.
/// </summary>
public record DataContracts(ImmutableArray<EntityDef> Entities)
{
    public static DataContracts Empty { get; } = new(ImmutableArray<EntityDef>.Empty);

    public static DataContracts Default { get; } = new(
        ImmutableArray.Create(
            new EntityDef("Job", EntityKind.Record, ImmutableArray.Create(
                new FieldDef("id", "string"),
                new FieldDef("title", "string"),
                new FieldDef("status", "enum"),
                new FieldDef("scheduledAt", "datetime"),
                new FieldDef("technicianId", "string"),
                new FieldDef("notes", "string?"))),
            new EntityDef("Technician", EntityKind.Record, ImmutableArray.Create(
                new FieldDef("id", "string"),
                new FieldDef("name", "string"),
                new FieldDef("skills", "string[]"),
                new FieldDef("available", "bool"),
                new FieldDef("currentLocation", "GeoPoint?"))),
            new EntityDef("Schedule", EntityKind.Record, ImmutableArray.Create(
                new FieldDef("date", "date"),
                new FieldDef("jobs", "Job[]"),
                new FieldDef("conflicts", "Conflict[]")))));
}

public record EntityDef(string Name, EntityKind Kind, ImmutableArray<FieldDef> Fields);

public record FieldDef(string Name, string TypeText);

public enum EntityKind { Record, Class, Struct }
