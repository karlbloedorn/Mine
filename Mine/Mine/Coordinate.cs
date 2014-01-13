using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mine
{
  public class Coordinate : IEquatable<Coordinate>
  {
    public float latitude;
    public float longitude;
    public float radial_distance;
    public Coordinate(float x, float y, float z)
    {
      this.latitude = x;
      this.longitude = y;
      this.radial_distance = z;
    }
    public static bool operator !=(Coordinate a, Coordinate b)
    {
      return Equals(a, b);
    }
    public static bool operator ==(Coordinate a, Coordinate b)
    {
      return Equals(a, b);
    }
    public override bool Equals(object obj){
      if( !(obj is Coordinate)){
        return false;
      }
      return Equals( obj as Coordinate);
    }
    public bool Equals(Coordinate other){
      return other.latitude == this.latitude && other.longitude == this.longitude && other.radial_distance == this.radial_distance;
    }
    public override int GetHashCode()
    {
      return (int) (latitude * longitude * radial_distance);
    }
    public override string ToString(){
      return "X: " + latitude + " Y: " + longitude + " Z:" + radial_distance;
    }
  }
}
