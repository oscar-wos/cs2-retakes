﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RetakesPlugin.Modules.Managers;

public class BreakerManager
{
    private readonly bool _shouldBreakBreakables;
    private readonly bool _shouldOpenDoors;

    public BreakerManager(bool? shouldBreakBreakables, bool? shouldOpenDoors)
    {
        _shouldBreakBreakables = shouldBreakBreakables ?? false;
        _shouldOpenDoors = shouldOpenDoors ?? false;
    }

    public void Handle()
    {
        if (_shouldBreakBreakables)
        {
            BreakBreakables();
        }

        if (_shouldOpenDoors)
        {
            OpenDoors();
        }
    }

    private static void BreakBreakables()
    {
        var breakableEntities = new List<(string designerName, string action, Type entityType)>
        {
            ("func_breakable", "Break", typeof(CBreakable)),
            ("func_breakable_surf", "Break", typeof(CBreakable)),
            ("prop.breakable.01", "Break", typeof(CBreakableProp)),
            ("prop.breakable.02", "Break", typeof(CBreakableProp))
        };

        breakableEntities.AddRange(
            Utilities.FindAllEntitiesByDesignerName<CBreakable>("func_breakable")
                .Select(entity => (entity.DesignerName, "Break", entity.GetType()))
        );

        breakableEntities.AddRange(
            Utilities.FindAllEntitiesByDesignerName<CBreakable>("func_breakable_surf")
                .Select(entity => (entity.DesignerName, "Break", entity.GetType()))
        );

        breakableEntities.AddRange(
            Utilities.FindAllEntitiesByDesignerName<CBreakableProp>("prop_dynamic")
                .Select(entity => (entity.DesignerName, "Break", entity.GetType()))
        );

        if (Server.MapName == "de_vertigo" || Server.MapName == "de_cache" || Server.MapName == "de_nuke")
        {
            breakableEntities.Add(("prop_dynamic", "Break", typeof(CDynamicProp)));
        }

        if (Server.MapName == "de_nuke")
        {
            breakableEntities.Add(("func_button", "Kill", typeof(CBaseButton)));
        }

        foreach (var (designerName, action, entityType) in breakableEntities)
        {
            IEnumerable<object> entities = entityType switch
            {
                Type et when et == typeof(CBreakable) => Utilities.FindAllEntitiesByDesignerName<CBreakable>(
                    designerName),
                Type et when et == typeof(CBreakableProp) => Utilities.FindAllEntitiesByDesignerName<CBreakableProp>(
                    designerName),
                Type et when et == typeof(CDynamicProp) => Utilities.FindAllEntitiesByDesignerName<CDynamicProp>(
                    designerName),
                Type et when et == typeof(CBaseButton) => Utilities.FindAllEntitiesByDesignerName<CBaseButton>(
                    designerName),
                _ => throw new InvalidOperationException("Unsupported entity type")
            };

            foreach (var entity in entities)
            {
                if (entity is CBreakable breakable)
                {
                    breakable.AcceptInput("Break");
                }
                else if (entity is CBreakableProp breakableProp)
                {
                    breakableProp.AcceptInput("Break");
                }
                else if (entity is CDynamicProp dynamicProp)
                {
                    dynamicProp.AcceptInput("Break");
                }
                else if (entity is CBaseButton baseButton)
                {
                    baseButton.AcceptInput("Break");
                }
            }
        }
    }

    private static void OpenDoors()
    {
        // TODO: It'll probably be more efficient to do it during the entity loop above.
        var doorEntities = Utilities.FindAllEntitiesByDesignerName<CPropDoorRotating>("prop_door_rotating");

        foreach (var doorEntity in doorEntities)
        {
            doorEntity.AcceptInput("open");
        }
    }
}