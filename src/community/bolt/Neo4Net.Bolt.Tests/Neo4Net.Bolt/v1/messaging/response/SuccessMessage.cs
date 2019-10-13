﻿/*
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
namespace Neo4Net.Bolt.v1.messaging.response
{
	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	public class SuccessMessage : ResponseMessage
	{
		 public const sbyte SIGNATURE = 0x70;
		 private readonly MapValue _metadata;

		 public SuccessMessage( MapValue metadata )
		 {
			  this._metadata = metadata;
		 }

		 public override sbyte Signature()
		 {
			  return SIGNATURE;
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

			  SuccessMessage that = ( SuccessMessage ) o;

			  return _metadata.Equals( that._metadata );

		 }

		 public override int GetHashCode()
		 {
			  return _metadata.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return "SUCCESS " + _metadata;
		 }

		 public virtual MapValue Meta()
		 {
			  return _metadata;
		 }
	}

}