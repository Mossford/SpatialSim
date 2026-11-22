namespace SpatialSim.Game.Rendering.StarRendering
{
    public static class StarManager
    {
        //stars are stored in bunch of positions, culled to view frustum, then projected down to screen space (compute shader?) then for screen space quad
        //render at back (first to render) then in fragment take screen space positions and apply a psf kernel over it (could also be compute shader to a texture?
    }
}