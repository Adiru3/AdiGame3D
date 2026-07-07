namespace Engine.Core.Entities
{
    /// <summary>
    /// Типы игровых объектов на сцене.
    /// </summary>
    public enum EntityType
    {
        // === Блоки ===
        Block        = 0,
        Stone        = 1,
        Wood         = 2,
        Glass        = 3,
        Metal        = 4,
        Brick        = 5,
        Grass        = 6,
        Sand         = 7,
        Water        = 8,
        Lava         = 9,
        Ice          = 10,
        Dirt         = 11,
        Model3D      = 12,

        // === Специальные объекты ===
        PlayerSpawn  = 100,
        Light        = 101,
        Trigger      = 102,
        Checkpoint   = 103,
        KillZone     = 104,
        SoundPoint   = 105,
        CameraWaypoint = 106,
    }
}
