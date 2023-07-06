using SharpGL;
using System.Collections.Generic;

namespace MvkClient.Renderer.Shaders
{
    /// <summary>
    /// Колекция шейдеров
    /// </summary>
    public class ShaderItems
    {
        /// <summary>
        /// Шейдер вокселей
        /// </summary>
        public ShaderVoxel ShVoxel { get; protected set; } = new ShaderVoxel();
        /// <summary>
        /// Шейдер вокселей с эффектом ветра
        /// </summary>
        public ShaderVoxelWind ShVoxelWind { get; protected set; } = new ShaderVoxelWind();
        /// <summary>
        /// Шейдер вокселей с эффектом волны
        /// </summary>
        public ShaderVoxelWave ShVoxelWave { get; protected set; } = new ShaderVoxelWave();

        public void Create(OpenGL gl)
        {
            ShVoxel.Create(gl, new Dictionary<uint, string> {
                { 0, "v_position" },
                { 1, "v_texCoord" },
                { 2, "v_rgbl" },
                { 3, "v_anim" }
            });

            ShVoxelWind.Create(gl, new Dictionary<uint, string> {
                { 0, "v_position" },
                { 1, "v_texCoord" },
                { 2, "v_rgbl" },
                { 3, "v_anim" }
            });

            ShVoxelWave.Create(gl, new Dictionary<uint, string> {
                { 0, "v_position" },
                { 1, "v_texCoord" },
                { 2, "v_rgbl" },
                { 3, "v_anim" }
            });
        }
    }
}
