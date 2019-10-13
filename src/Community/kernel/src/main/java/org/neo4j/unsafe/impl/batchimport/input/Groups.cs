using System.Collections.Generic;

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

	/// <summary>
	/// Mapping from name to <seealso cref="Group"/>. Assigns proper <seealso cref="Group.id() ids"/> to created groups.
	/// </summary>
	public class Groups
	{
		 internal const int LOWEST_NONGLOBAL_ID = 1;

		 private readonly IDictionary<string, Group> _byName = new Dictionary<string, Group>();
		 private readonly IList<Group> _byId = new IList<Group> { Group_Fields.Global };
		 private int _nextId = LOWEST_NONGLOBAL_ID;

		 /// <param name="name"> group name or {@code null} for a <seealso cref="Group.GLOBAL global group"/>. </param>
		 /// <returns> <seealso cref="Group"/> for the given name. If the group doesn't already exist it will be created
		 /// with a new id. If {@code name} is {@code null} then the <seealso cref="Group.GLOBAL global group"/> is returned.
		 /// This method also prevents mixing global and non-global groups, i.e. if first call is {@code null},
		 /// then consecutive calls have to specify {@code null} name as well. The same holds true for non-null values. </returns>
		 public virtual Group GetOrCreate( string name )
		 {
			 lock ( this )
			 {
				  if ( IsGlobalGroup( name ) )
				  {
						return Group_Fields.Global;
				  }
      
				  Group group = _byName[name];
				  if ( group == null )
				  {
						_byName[name] = group = new Group_Adapter( _nextId++, name );
						_byId.Add( group );
				  }
				  return group;
			 }
		 }

		 private static bool IsGlobalGroup( string name )
		 {
			  return string.ReferenceEquals( name, null ) || Group_Fields.Global.name().Equals(name);
		 }

		 public virtual Group Get( string name )
		 {
			 lock ( this )
			 {
				  if ( IsGlobalGroup( name ) )
				  {
						return Group_Fields.Global;
				  }
      
				  Group group = _byName[name];
				  if ( group == null )
				  {
						throw new HeaderException( "Group '" + name + "' not found. Available groups are: " + GroupNames() );
				  }
				  return group;
			 }
		 }

		 public virtual Group Get( int id )
		 {
			  if ( id < 0 || id >= _byId.Count )
			  {
					throw new HeaderException( "Group with id " + id + " not found" );
			  }
			  return _byId[id];
		 }

		 private string GroupNames()
		 {
			  return Arrays.ToString( _byName.Keys.toArray( new string[_byName.Keys.Count] ) );
		 }

		 public virtual int Size()
		 {
			  return _nextId;
		 }
	}

}