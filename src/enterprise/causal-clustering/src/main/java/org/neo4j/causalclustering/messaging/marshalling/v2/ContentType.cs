using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.messaging.marshalling.v2
{
	public sealed class ContentType
	{
		 public static readonly ContentType ContentType = new ContentType( "ContentType", InnerEnum.ContentType, ( sbyte ) 0 );
		 public static readonly ContentType ReplicatedContent = new ContentType( "ReplicatedContent", InnerEnum.ReplicatedContent, ( sbyte ) 1 );
		 public static readonly ContentType RaftLogEntryTerms = new ContentType( "RaftLogEntryTerms", InnerEnum.RaftLogEntryTerms, ( sbyte ) 2 );
		 public static readonly ContentType Message = new ContentType( "Message", InnerEnum.Message, ( sbyte ) 3 );

		 private static readonly IList<ContentType> valueList = new List<ContentType>();

		 static ContentType()
		 {
			 valueList.Add( ContentType );
			 valueList.Add( ReplicatedContent );
			 valueList.Add( RaftLogEntryTerms );
			 valueList.Add( Message );
		 }

		 public enum InnerEnum
		 {
			 ContentType,
			 ReplicatedContent,
			 RaftLogEntryTerms,
			 Message
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private readonly sbyte messageCode;

		 internal ContentType( string name, InnerEnum innerEnum, sbyte messageCode )
		 {
			  this._messageCode = messageCode;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public sbyte Get()
		 {
			  return _messageCode;
		 }

		public static IList<ContentType> values()
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

		public static ContentType valueOf( string name )
		{
			foreach ( ContentType enumInstance in ContentType.valueList )
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