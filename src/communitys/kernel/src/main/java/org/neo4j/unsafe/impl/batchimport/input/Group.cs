/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;

	/// <summary>
	/// Group of <seealso cref="InputEntity inputs"/>. Used primarily in <seealso cref="IdMapper"/> for supporting multiple
	/// id groups within the same index.
	/// </summary>
	public interface Group
	{
		 /// <returns> id of this group, used for identifying this group. </returns>
		 int Id();

		 /// <returns> the name of this group. </returns>
		 string Name();

		 /// <returns> <seealso cref="name()"/>. </returns>
		 String ();
	}

	public static class Group_Fields
	{
		 public static readonly Group Global = new Adapter( 0, "global id space" );
	}

	 public class Group_Adapter : Group
	 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly int IdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		  internal readonly string NameConflict;

		  public Group_Adapter( int id, string name )
		  {
				this.IdConflict = id;
				this.NameConflict = name;
		  }

		  public override int Id()
		  {
				return IdConflict;
		  }

		  public override string Name()
		  {
				return NameConflict;
		  }

		  public override string ToString()
		  {
				return "(" + NameConflict + "," + IdConflict + ")";
		  }

		  public override int GetHashCode()
		  {
				const int prime = 31;
				int result = 1;
				result = prime * result + IdConflict;
				return result;
		  }

		  public override bool Equals( object obj )
		  {
				return obj is Group && ( ( Group )obj ).Id() == IdConflict;
		  }
	 }

}