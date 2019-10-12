/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.cluster.protocol.election
{
	/// <summary>
	/// Role that an instance can have in a cluster.
	/// </summary>
	public class ElectionRole
	{
		 private string _name;

		 public ElectionRole( string name )
		 {
			  this._name = name;
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return _name;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  ElectionRole that = ( ElectionRole ) o;

			  return !( !string.ReferenceEquals( _name, null ) ?!_name.Equals( that._name ) :!string.ReferenceEquals( that._name, null ) );

		 }

		 public override int GetHashCode()
		 {
			  return !string.ReferenceEquals( _name, null ) ? _name.GetHashCode() : 0;
		 }
	}

}