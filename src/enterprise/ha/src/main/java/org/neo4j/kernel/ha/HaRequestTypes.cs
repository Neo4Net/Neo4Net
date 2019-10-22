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
namespace Neo4Net.Kernel.ha
{
	using RequestType = Neo4Net.com.RequestType;

	public interface HaRequestTypes
	{

		 RequestType Type( HaRequestTypes_Type type );

		 RequestType Type( sbyte id );
	}

	 public sealed class HaRequestTypes_Type
	 {
		  // Order here is vital since it represents the request type (byte) ordinal which is communicated
		  // as part of the HA protocol.
		  public static readonly HaRequestTypes_Type AllocateIds = new HaRequestTypes_Type( "AllocateIds", InnerEnum.AllocateIds );
		  public static readonly HaRequestTypes_Type CreateRelationshipType = new HaRequestTypes_Type( "CreateRelationshipType", InnerEnum.CreateRelationshipType );
		  public static readonly HaRequestTypes_Type AcquireExclusiveLock = new HaRequestTypes_Type( "AcquireExclusiveLock", InnerEnum.AcquireExclusiveLock );
		  public static readonly HaRequestTypes_Type AcquireSharedLock = new HaRequestTypes_Type( "AcquireSharedLock", InnerEnum.AcquireSharedLock );
		  public static readonly HaRequestTypes_Type Commit = new HaRequestTypes_Type( "Commit", InnerEnum.Commit );
		  public static readonly HaRequestTypes_Type PullUpdates = new HaRequestTypes_Type( "PullUpdates", InnerEnum.PullUpdates );
		  public static readonly HaRequestTypes_Type EndLockSession = new HaRequestTypes_Type( "EndLockSession", InnerEnum.EndLockSession );
		  public static readonly HaRequestTypes_Type Handshake = new HaRequestTypes_Type( "Handshake", InnerEnum.Handshake );
		  public static readonly HaRequestTypes_Type CopyStore = new HaRequestTypes_Type( "CopyStore", InnerEnum.CopyStore );
		  public static readonly HaRequestTypes_Type CopyTransactions = new HaRequestTypes_Type( "CopyTransactions", InnerEnum.CopyTransactions );
		  public static readonly HaRequestTypes_Type NewLockSession = new HaRequestTypes_Type( "NewLockSession", InnerEnum.NewLockSession );
		  public static readonly HaRequestTypes_Type PushTransactions = new HaRequestTypes_Type( "PushTransactions", InnerEnum.PushTransactions );
		  public static readonly HaRequestTypes_Type CreatePropertyKey = new HaRequestTypes_Type( "CreatePropertyKey", InnerEnum.CreatePropertyKey );
		  public static readonly HaRequestTypes_Type CreateLabel = new HaRequestTypes_Type( "CreateLabel", InnerEnum.CreateLabel );

		  private static readonly IList<HaRequestTypes_Type> valueList = new List<HaRequestTypes_Type>();

		  static HaRequestTypes_Type()
		  {
			  valueList.Add( AllocateIds );
			  valueList.Add( CreateRelationshipType );
			  valueList.Add( AcquireExclusiveLock );
			  valueList.Add( AcquireSharedLock );
			  valueList.Add( Commit );
			  valueList.Add( PullUpdates );
			  valueList.Add( EndLockSession );
			  valueList.Add( Handshake );
			  valueList.Add( CopyStore );
			  valueList.Add( CopyTransactions );
			  valueList.Add( NewLockSession );
			  valueList.Add( PushTransactions );
			  valueList.Add( CreatePropertyKey );
			  valueList.Add( CreateLabel );
		  }

		  public enum InnerEnum
		  {
			  AllocateIds,
			  CreateRelationshipType,
			  AcquireExclusiveLock,
			  AcquireSharedLock,
			  Commit,
			  PullUpdates,
			  EndLockSession,
			  Handshake,
			  CopyStore,
			  CopyTransactions,
			  NewLockSession,
			  PushTransactions,
			  CreatePropertyKey,
			  CreateLabel
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  private HaRequestTypes_Type( string name, InnerEnum innerEnum )
		  {
			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		  public bool Is( Neo4Net.com.RequestType type )
		  {
				return type.Id() == ordinal();
		  }

		 public static IList<HaRequestTypes_Type> values()
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

		 public static HaRequestTypes_Type valueOf( string name )
		 {
			 foreach ( HaRequestTypes_Type enumInstance in HaRequestTypes_Type.valueList )
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