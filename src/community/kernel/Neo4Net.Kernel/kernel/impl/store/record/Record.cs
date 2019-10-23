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
namespace Neo4Net.Kernel.Impl.Store.Records
{
	/// <summary>
	/// Various constants used in records for different stores.
	/// </summary>
	public sealed class Record
	{
		 /// <summary>
		 /// Generic value of a reference not pointing to anything.
		 /// </summary>
		 public static readonly Record NullReference = new Record( "NullReference", InnerEnum.NullReference, ( sbyte ) - 1, -1 );

		 public static readonly Record NotInUse = new Record( "NotInUse", InnerEnum.NotInUse, ( sbyte ) 0, 0 );
		 public static readonly Record InUse = new Record( "InUse", InnerEnum.InUse, ( sbyte ) 1, 1 );
		 public static readonly Record FirstInChain = new Record( "FirstInChain", InnerEnum.FirstInChain, ( sbyte ) 2, 2 );
		 public static readonly Record Reserved = new Record( "Reserved", InnerEnum.Reserved, ( sbyte ) - 1, -1 );
		 public static readonly Record NoNextProperty = new Record( "NoNextProperty", InnerEnum.NoNextProperty, NULL_REFERENCE );
		 public static readonly Record NoPreviousProperty = new Record( "NoPreviousProperty", InnerEnum.NoPreviousProperty, NULL_REFERENCE );
		 public static readonly Record NoNextRelationship = new Record( "NoNextRelationship", InnerEnum.NoNextRelationship, NULL_REFERENCE );
		 public static readonly Record NoPrevRelationship = new Record( "NoPrevRelationship", InnerEnum.NoPrevRelationship, NULL_REFERENCE );
		 public static readonly Record NoNextBlock = new Record( "NoNextBlock", InnerEnum.NoNextBlock, NULL_REFERENCE );

		 public static readonly Record NodeProperty = new Record( "NodeProperty", InnerEnum.NodeProperty, ( sbyte ) 0, 0 );
		 public static readonly Record RelProperty = new Record( "RelProperty", InnerEnum.RelProperty, ( sbyte ) 2, 2 );

		 public static readonly Record NoLabelsField = new Record( "NoLabelsField", InnerEnum.NoLabelsField, ( sbyte )0, 0 );

		 private static readonly IList<Record> valueList = new List<Record>();

		 static Record()
		 {
			 valueList.Add( NullReference );
			 valueList.Add( NotInUse );
			 valueList.Add( InUse );
			 valueList.Add( FirstInChain );
			 valueList.Add( Reserved );
			 valueList.Add( NoNextProperty );
			 valueList.Add( NoPreviousProperty );
			 valueList.Add( NoNextRelationship );
			 valueList.Add( NoPrevRelationship );
			 valueList.Add( NoNextBlock );
			 valueList.Add( NodeProperty );
			 valueList.Add( RelProperty );
			 valueList.Add( NoLabelsField );
		 }

		 public enum InnerEnum
		 {
			 NullReference,
			 NotInUse,
			 InUse,
			 FirstInChain,
			 Reserved,
			 NoNextProperty,
			 NoPreviousProperty,
			 NoNextRelationship,
			 NoPrevRelationship,
			 NoNextBlock,
			 NodeProperty,
			 RelProperty,
			 NoLabelsField
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Public const;
		 internal Public const;
		 internal Public const;
		 internal Public const;

		 internal Private readonly;
		 internal Private readonly;

		 internal Record( string name, InnerEnum innerEnum, Record from ) : this( from._byteValue, from._intValue )
		 {

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal Record( string name, InnerEnum innerEnum, sbyte byteValue, int intValue )
		 {
			  this._byteValue = byteValue;
			  this._intValue = intValue;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <summary>
		 /// Returns a byte value representation for this record type.
		 /// </summary>
		 /// <returns> The byte value for this record type </returns>
		 public sbyte ByteValue()
		 {
			  return _byteValue;
		 }

		 /// <summary>
		 /// Returns a int value representation for this record type.
		 /// </summary>
		 /// <returns> The int value for this record type </returns>
		 public int IntValue()
		 {
			  return _intValue;
		 }

		 public long LongValue()
		 {
			  return _intValue;
		 }

		 public bool Is( long value )
		 {
			  return value == _intValue;
		 }

		public static IList<Record> values()
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

		public static Record ValueOf( string name )
		{
			foreach ( Record enumInstance in Record.valueList )
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