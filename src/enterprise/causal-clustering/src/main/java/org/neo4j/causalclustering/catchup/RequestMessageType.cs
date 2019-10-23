using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup
{
	using Message = Neo4Net.causalclustering.messaging.Message;

	public sealed class RequestMessageType : Message
	{
		 public static readonly RequestMessageType TxPullRequest = new RequestMessageType( "TxPullRequest", InnerEnum.TxPullRequest, ( sbyte ) 1 );
		 public static readonly RequestMessageType Store = new RequestMessageType( "Store", InnerEnum.Store, ( sbyte ) 2 );
		 public static readonly RequestMessageType CoreSnapshot = new RequestMessageType( "CoreSnapshot", InnerEnum.CoreSnapshot, ( sbyte ) 3 );
		 public static readonly RequestMessageType StoreId = new RequestMessageType( "StoreId", InnerEnum.StoreId, ( sbyte ) 4 );
		 public static readonly RequestMessageType PrepareStoreCopy = new RequestMessageType( "PrepareStoreCopy", InnerEnum.PrepareStoreCopy, ( sbyte ) 5 );
		 public static readonly RequestMessageType StoreFile = new RequestMessageType( "StoreFile", InnerEnum.StoreFile, ( sbyte ) 6 );
		 public static readonly RequestMessageType IndexSnapshot = new RequestMessageType( "IndexSnapshot", InnerEnum.IndexSnapshot, ( sbyte ) 7 );
		 public static readonly RequestMessageType Unknown = new RequestMessageType( "Unknown", InnerEnum.Unknown, unchecked( ( sbyte ) 404 ) );

		 private static readonly IList<RequestMessageType> valueList = new List<RequestMessageType>();

		 static RequestMessageType()
		 {
			 valueList.Add( TxPullRequest );
			 valueList.Add( Store );
			 valueList.Add( CoreSnapshot );
			 valueList.Add( StoreId );
			 valueList.Add( PrepareStoreCopy );
			 valueList.Add( StoreFile );
			 valueList.Add( IndexSnapshot );
			 valueList.Add( Unknown );
		 }

		 public enum InnerEnum
		 {
			 TxPullRequest,
			 Store,
			 CoreSnapshot,
			 StoreId,
			 PrepareStoreCopy,
			 StoreFile,
			 IndexSnapshot,
			 Unknown
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private byte;

		 internal RequestMessageType( string name, InnerEnum innerEnum, sbyte messageType )
		 {
			  this._messageType = messageType;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public static RequestMessageType From( sbyte b )
		 {
			  foreach ( RequestMessageType responseMessageType in values() )
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
			  return format( "RequestMessageType{messageType=%s}", _messageType );
		 }

		public static IList<RequestMessageType> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static RequestMessageType ValueOf( string name )
		{
			foreach ( RequestMessageType enumInstance in RequestMessageType.valueList )
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