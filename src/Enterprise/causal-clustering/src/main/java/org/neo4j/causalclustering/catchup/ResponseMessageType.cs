using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.catchup
{

	public sealed class ResponseMessageType
	{
		 public static readonly ResponseMessageType Tx = new ResponseMessageType( "Tx", InnerEnum.Tx, ( sbyte ) 1 );
		 public static readonly ResponseMessageType StoreId = new ResponseMessageType( "StoreId", InnerEnum.StoreId, ( sbyte ) 2 );
		 public static readonly ResponseMessageType File = new ResponseMessageType( "File", InnerEnum.File, ( sbyte ) 3 );
		 public static readonly ResponseMessageType StoreCopyFinished = new ResponseMessageType( "StoreCopyFinished", InnerEnum.StoreCopyFinished, ( sbyte ) 4 );
		 public static readonly ResponseMessageType CoreSnapshot = new ResponseMessageType( "CoreSnapshot", InnerEnum.CoreSnapshot, ( sbyte ) 5 );
		 public static readonly ResponseMessageType TxStreamFinished = new ResponseMessageType( "TxStreamFinished", InnerEnum.TxStreamFinished, ( sbyte ) 6 );
		 public static readonly ResponseMessageType PrepareStoreCopyResponse = new ResponseMessageType( "PrepareStoreCopyResponse", InnerEnum.PrepareStoreCopyResponse, ( sbyte ) 7 );
		 public static readonly ResponseMessageType IndexSnapshotResponse = new ResponseMessageType( "IndexSnapshotResponse", InnerEnum.IndexSnapshotResponse, ( sbyte ) 8 );
		 public static readonly ResponseMessageType Unknown = new ResponseMessageType( "Unknown", InnerEnum.Unknown, unchecked( ( sbyte ) 200 ) );

		 private static readonly IList<ResponseMessageType> valueList = new List<ResponseMessageType>();

		 static ResponseMessageType()
		 {
			 valueList.Add( Tx );
			 valueList.Add( StoreId );
			 valueList.Add( File );
			 valueList.Add( StoreCopyFinished );
			 valueList.Add( CoreSnapshot );
			 valueList.Add( TxStreamFinished );
			 valueList.Add( PrepareStoreCopyResponse );
			 valueList.Add( IndexSnapshotResponse );
			 valueList.Add( Unknown );
		 }

		 public enum InnerEnum
		 {
			 Tx,
			 StoreId,
			 File,
			 StoreCopyFinished,
			 CoreSnapshot,
			 TxStreamFinished,
			 PrepareStoreCopyResponse,
			 IndexSnapshotResponse,
			 Unknown
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private byte;

		 internal ResponseMessageType( string name, InnerEnum innerEnum, sbyte messageType )
		 {
			  this._messageType = messageType;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public static ResponseMessageType From( sbyte b )
		 {
			  foreach ( ResponseMessageType responseMessageType in values() )
			  {
					if ( responseMessageType._messageType == b )
					{
						 return responseMessageType;
					}
			  }
			  return UNKNOWN;
		 }

		 public sbyte MessageType()
		 {
			  return _messageType;
		 }

		 public override string ToString()
		 {
			  return format( "ResponseMessageType{messageType=%s}", _messageType );
		 }

		public static IList<ResponseMessageType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static ResponseMessageType valueOf( string name )
		{
			foreach ( ResponseMessageType enumInstance in ResponseMessageType.valueList )
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