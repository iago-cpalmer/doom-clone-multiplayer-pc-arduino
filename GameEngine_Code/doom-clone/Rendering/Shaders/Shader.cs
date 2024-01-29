using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace doom_clone.Rendering.Shaders
{
    // A simple class meant to help create shaders.
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;
        private readonly Dictionary<string, int> _uniformBlockLocation;

        public static Dictionary<string, int> UboBindings = new Dictionary<string, int>();
        private static int _uboCounter = 0;

        // This is how you create a simple shader.
        // Shaders are written in GLSL, which is a language very similar to C in its semantics.
        // The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
        // A commented example of GLSL can be found in shader.vert.
        public Shader(string vertPath, string fragPath)
        {
            vertPath = "..\\..\\..\\" + vertPath;
            fragPath = "..\\..\\..\\" + fragPath;
            
            // Load vertex shader and compile
            string shaderSource = File.ReadAllText(vertPath);
            string sourcePath = vertPath.Substring(0, vertPath.LastIndexOf("/"));
            ParseIncludes(ref shaderSource, ref sourcePath);
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            
            // Now, bind the GLSL source code
            GL.ShaderSource(vertexShader, shaderSource);

            // And then compile
            CompileShader(vertexShader);
            sourcePath = fragPath.Substring(0, fragPath.LastIndexOf("/"));
            shaderSource = File.ReadAllText(fragPath);
            ParseIncludes(ref shaderSource, ref sourcePath);
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            Handle = GL.CreateProgram();

          
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // Handle uniforms
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniformBlocks, out int numberOfUBO);
            _uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms
            for (int i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                string key = GL.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                int location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                _uniformLocations.Add(key, location);

                Console.WriteLine("UV added: " + key + ", location: " + location);
            }
            _uniformBlockLocation = new Dictionary<string, int>();
            for (int i = 0; i < numberOfUBO; i++)
            { 
                GL.GetActiveUniformBlock(Handle, i, ActiveUniformBlockParameter.UniformBlockDataSize, out int bufferSize);
                GL.GetActiveUniformBlockName(Handle, i, bufferSize, out _, out string bufferName);
                int location = GL.GetUniformBlockIndex(Handle, bufferName);
                _uniformBlockLocation.Add(bufferName, location);
                int bindingPoint = GetUboBindingPoint(bufferName);
                GL.UniformBlockBinding(Handle, location, bindingPoint);
                Console.WriteLine("UBO added: " + bufferName + ", location: " + location + ", bindingPoint: " + bindingPoint + ", bufferSize: " + bufferSize);
            }
        }

        private void ParseIncludes(ref string source, ref string sourcePath)
        {
            int lastIncludeIndex = source.IndexOf("#include", 0);
            int jumpOfLineIndex = 0;
            string line = "";
            List<string> filesToInclude = new List<string>();
            List<int> fileIndices = new List<int>();
            while(lastIncludeIndex!=-1)
            {
                jumpOfLineIndex = source.IndexOf('\n', lastIncludeIndex);
                fileIndices.Insert(0, lastIncludeIndex);
                line = source.Substring(lastIncludeIndex, jumpOfLineIndex);
                int startingIndex = source.IndexOf('<', lastIncludeIndex) + 1;
                string filePath = source.Substring(startingIndex, source.IndexOf('>', startingIndex) - startingIndex);
                filesToInclude.Insert(0, filePath);
                lastIncludeIndex = source.IndexOf("#include", jumpOfLineIndex);
            }
            int i = 0;
            foreach(string file in filesToInclude)
            {
                source = source.Insert(fileIndices[i++] - 2, File.ReadAllText(sourcePath + "/"+ file));
            }
        }

        private int GetUboBindingPoint(string uboName)
        {
            if(!UboBindings.TryGetValue(uboName, out int bindingPoint))
            {
                
                UboBindings.Add(uboName, _uboCounter);
                Console.WriteLine("UBO Not found, creating one at binding point: " + _uboCounter);
                return _uboCounter++;
            }
            return bindingPoint;
        }

        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
        // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Uniform setters
        // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        // You use VBOs for vertex-related data, and uniforms for almost everything else.

        // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //     1. Bind the program you want to set the uniform on
        //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
        //     3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }
    }
}