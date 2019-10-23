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
namespace Neo4Net.Bolt.v1.messaging
{

	/// <summary>
	/// Enumeration representing all defined Bolt response messages.
	/// Also contains the signature byte with which the message is
	/// encoded on the wire.
	/// </summary>
	public sealed class BoltResponseMessage
	{
		 public static readonly BoltResponseMessage Success = new BoltResponseMessage( "Success", InnerEnum.Success, 0x70 );
		 public static readonly BoltResponseMessage Record = new BoltResponseMessage( "Record", InnerEnum.Record, 0x71 );
		 public static readonly BoltResponseMessage Ignored = new BoltResponseMessage( "Ignored", InnerEnum.Ignored, 0x7E );
		 public static readonly BoltResponseMessage Failure = new BoltResponseMessage( "Failure", InnerEnum.Failure, 0x7F );

		 private static readonly IList<BoltResponseMessage> valueList = new List<BoltResponseMessage>();

		 public enum InnerEnum
		 {
			 Success,
			 Record,
			 Ignored,
			 Failure
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private static;
		 static BoltResponseMessage()
		 {
			  foreach ( BoltResponseMessage value in values() )
			  {
					_valuesBySignature[value.Signature()] = value;
			  }

			 valueList.Add( Success );
			 valueList.Add( Record );
			 valueList.Add( Ignored );
			 valueList.Add( Failure );
		 }

		 /// <summary>
		 /// Obtain a response message by signature.
		 /// </summary>
		 /// <param name="signature"> the signature byte to look up </param>
		 /// <returns> the appropriate message instance </returns>
		 /// <exception cref="IllegalArgumentException"> if no such message exists </exception>
		 public static BoltResponseMessage WithSignature( int signature )
		 {
			  BoltResponseMessage message = _valuesBySignature[signature];
			  if ( message == null )
			  {
					throw new System.ArgumentException( format( "No message with signature %d", signature ) );
			  }
			  return message;
		 }

		 internal Private readonly;

		 internal BoltResponseMessage( string name, InnerEnum innerEnum, int signature )
		 {
			  this._signature = ( sbyte ) signature;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public sbyte Signature()
		 {
			  return _signature;
		 }


		public static IList<BoltResponseMessage> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static BoltResponseMessage ValueOf( string name )
		{
			foreach ( BoltResponseMessage enumInstance in BoltResponseMessage.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}