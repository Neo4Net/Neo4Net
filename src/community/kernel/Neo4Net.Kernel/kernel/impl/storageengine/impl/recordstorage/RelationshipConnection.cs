﻿using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

	internal abstract class RelationshipConnection
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       START_PREV { long get(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.isFirstInFirstChain() ? org.Neo4Net.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.intValue() : rel.getFirstPrevRel(); } void set(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel, long id, boolean isFirst) { rel.setFirstPrevRel(id); rel.setFirstInFirstChain(isFirst); } RelationshipConnection otherSide() { return START_NEXT; } long compareNode(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.getFirstNode(); } RelationshipConnection start() { return this; } RelationshipConnection end() { return END_PREV; } boolean isFirstInChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.isFirstInFirstChain(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       START_NEXT { long get(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.getFirstNextRel(); } void set(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel, long id, boolean isFirst) { rel.setFirstNextRel(id); } RelationshipConnection otherSide() { return START_PREV; } long compareNode(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.getFirstNode(); } RelationshipConnection start() { return this; } RelationshipConnection end() { return END_NEXT; } boolean isFirstInChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.isFirstInFirstChain(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       END_PREV { long get(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.isFirstInSecondChain() ? org.Neo4Net.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP.intValue() : rel.getSecondPrevRel(); } void set(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel, long id, boolean isFirst) { rel.setSecondPrevRel(id); rel.setFirstInSecondChain(isFirst); } RelationshipConnection otherSide() { return END_NEXT; } long compareNode(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.getSecondNode(); } RelationshipConnection start() { return START_PREV; } RelationshipConnection end() { return this; } boolean isFirstInChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.isFirstInSecondChain(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       END_NEXT { long get(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.getSecondNextRel(); } void set(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel, long id, boolean isFirst) { rel.setSecondNextRel(id); } RelationshipConnection otherSide() { return END_PREV; } long compareNode(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.getSecondNode(); } RelationshipConnection start() { return START_NEXT; } RelationshipConnection end() { return this; } boolean isFirstInChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord rel) { return rel.isFirstInSecondChain(); } };

		 private static readonly IList<RelationshipConnection> valueList = new List<RelationshipConnection>();

		 static RelationshipConnection()
		 {
			 valueList.Add( START_PREV );
			 valueList.Add( START_NEXT );
			 valueList.Add( END_PREV );
			 valueList.Add( END_NEXT );
		 }

		 public enum InnerEnum
		 {
			 START_PREV,
			 START_NEXT,
			 END_PREV,
			 END_NEXT
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private RelationshipConnection( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal abstract long get( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord rel );

		 internal abstract bool isFirstInChain( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord rel );

		 internal abstract void set( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord rel, long id, bool isFirst );

		 internal abstract long compareNode( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord rel );

		 internal abstract RelationshipConnection otherSide();

		 internal abstract RelationshipConnection start();

		 internal abstract RelationshipConnection end();

		public static IList<RelationshipConnection> values()
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

		public static RelationshipConnection ValueOf( string name )
		{
			foreach ( RelationshipConnection enumInstance in RelationshipConnection.valueList )
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