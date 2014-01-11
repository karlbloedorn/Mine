using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mine
{
  public class Point3 : IEquatable<Point3>
  {
    public int X;
    public int Y;
    public int Z;
    public Point3(int x, int y, int z)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
    }
    public static bool operator !=(Point3 a, Point3 b)
    {
      return Equals(a, b);
    }
    public static bool operator ==(Point3 a, Point3 b)
    {
      return Equals(a, b);
    }
    public override bool Equals(object obj){
      if( !(obj is Point3)){
        return false;
      }
      return Equals( obj as Point3);
    }
    public bool Equals(Point3 other){
      return other.X == this.X && other.Y == this.Y && other.Z == this.Z;
    }
    public override int GetHashCode(){
      return X * Y * Z;
    }
    public override string ToString(){
      return "X: " + X + " Y: " + Y + " Z:" + Z;
    }
  }
}
