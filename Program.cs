using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Obj2Sgdk
{
    public class VXT3
    {
        public VXT3(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
    public class QUAD4
    {
        public QUAD4(VXT3 nrm)
        {
            
            NRM = nrm;
        }
        public VXT3 NRM { get; set; }
        public List<int> Q = new List<int>();
    }
    class Program
    {
        static void Main(string[] args)
        {
            string precision = "F2";
            string outBuffer = "#include \"genesis.h\"\n";
            CultureInfo inf = CultureInfo.InvariantCulture;
            List<VXT3> vertices = new List<VXT3>();
            List<VXT3> normals = new List<VXT3>();
            List<QUAD4> quads = new List<QUAD4>();
            if (args.Length > 0)
            {
                var path = args[0];
                Console.WriteLine(path);
                string objData = File.ReadAllText(path);
                using (var reader = new StringReader(objData))
                {
                    for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                    {
                        string[] buffer = line.Split(' ');
                        if (line.StartsWith("v "))
                        {
                            vertices.Add(new VXT3(double.Parse(buffer[1], inf), double.Parse(buffer[2], inf), double.Parse(buffer[3], inf)));
                        }
                        else if (line.StartsWith("vn "))
                        {
                            normals.Add(new VXT3(double.Parse(buffer[1], inf), double.Parse(buffer[2], inf), double.Parse(buffer[3], inf)));
                        }
                        else if (line.StartsWith("f "))
                        {
                            QUAD4 q = new QUAD4(null);
                            for (byte b = 1; b < buffer.Length; b++)
                            {
                                string[] face = buffer[b].Split(new string[] { "//" }, StringSplitOptions.None);
                                q.NRM = normals[Convert.ToInt32(face[1])-1];
                                q.Q.Add(Convert.ToInt32(face[0])-1);
                            }
                            quads.Add(q);
                        }
                    }
                }

                outBuffer += "const Vect3D_f16 cube_coord[" + vertices.Count + "] =\n{\n";
                for (byte v=0; v<vertices.Count;v++)
                {
                    outBuffer += "  {FIX16(" + vertices[v].X.ToString(precision,inf) + "),FIX16(" + vertices[v].Y.ToString(precision,inf) + "),FIX16(" + vertices[v].Z.ToString(precision,inf);
                    outBuffer += v>= (vertices.Count - 1)? ")}\n":")},\n";
                }
                outBuffer += "};\n";

                outBuffer += "u16 cube_poly_ind[" + quads.Count + " * 4] =\n{\n";
                for (byte q = 0; q < quads.Count; q++)
                {
                    outBuffer += "  " + quads[q].Q[0].ToString()+','+ quads[q].Q[1].ToString() + ',' + quads[q].Q[2].ToString() + ',' + quads[q].Q[3].ToString();
                    outBuffer += q >= (quads.Count - 1) ? "\n" : ",\n";
                }
                outBuffer += "};\n";

                outBuffer += "const Vect3D_f16 cube_face_norm[" + quads.Count + "] =\n{\n";
                for (byte n = 0; n < quads.Count; n++)
                {                               
                    outBuffer += "  {FIX16(" + quads[n].NRM.X.ToString(precision,inf) + "),FIX16(" + quads[n].NRM.Y.ToString(precision,inf) + "),FIX16(" + quads[n].NRM.Z.ToString(precision,inf);
                    outBuffer += n >= (quads.Count - 1) ? ")}\n" : ")},\n";
                }
                outBuffer += "};\n";

                Console.WriteLine(outBuffer);
                File.WriteAllText("meshs.c", outBuffer);
                Console.WriteLine("Completed! Press any key to continue...");
                Console.ReadLine();
            }
        }
    }
}
