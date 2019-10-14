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
namespace Neo4Net.Bolt.v1.messaging.response
{
	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using QueryResult = Neo4Net.Cypher.result.QueryResult;
	using AnyValue = Neo4Net.Values.AnyValue;

	public class RecordMessage : ResponseMessage
	{
		 public const sbyte SIGNATURE = 0x71;
		 private readonly Neo4Net.Cypher.result.QueryResult_Record _value;

		 public RecordMessage( Neo4Net.Cypher.result.QueryResult_Record record )
		 {
			  this._value = record;
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

			  RecordMessage that = ( RecordMessage ) o;

			  return _value == null ? that._value == null : _value.Equals( that._value );
		 }

		 public override int GetHashCode()
		 {
			  return _value.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return "RECORD " + _value;
		 }

		 public virtual Neo4Net.Cypher.result.QueryResult_Record Record()
		 {
			  return _value;
		 }

		 public virtual AnyValue[] Fields()
		 {
			  return _value.fields();
		 }
	}

}