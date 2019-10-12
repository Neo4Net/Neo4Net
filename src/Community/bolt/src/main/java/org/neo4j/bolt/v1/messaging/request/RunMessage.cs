/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Bolt.v1.messaging.request
{

	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

	public class RunMessage : RequestMessage
	{
		 public const sbyte SIGNATURE = 0x10;

		 private readonly string _statement;
		 private readonly MapValue @params;

		 public RunMessage( string statement ) : this( statement, VirtualValues.EMPTY_MAP )
		 {
		 }

		 public RunMessage( string statement, MapValue @params )
		 {
			  this._statement = requireNonNull( statement );
			  this.@params = requireNonNull( @params );
		 }

		 public virtual string Statement()
		 {
			  return _statement;
		 }

		 public virtual MapValue Params()
		 {
			  return @params;
		 }

		 public override bool SafeToProcessInAnyState()
		 {
			  return false;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  RunMessage that = ( RunMessage ) o;
			  return Objects.Equals( _statement, that._statement ) && Objects.Equals( @params, that.@params );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _statement, @params );
		 }

		 public override string ToString()
		 {
			  return "RUN " + _statement + ' ' + @params;
		 }
	}

}