﻿using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.stresstests
{
	public abstract class Validations
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ConsistencyCheck { Validation create(Resources resources) { return new ConsistencyCheck(resources); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       IdReuseUniqueFreeIds { Validation create(Resources resources) { return new IdReuse.UniqueFreeIds(resources); } };

		 private static readonly IList<Validations> valueList = new List<Validations>();

		 static Validations()
		 {
			 valueList.Add( ConsistencyCheck );
			 valueList.Add( IdReuseUniqueFreeIds );
		 }

		 public enum InnerEnum
		 {
			 ConsistencyCheck,
			 IdReuseUniqueFreeIds
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private Validations( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal abstract Validation create( Resources resources );

		public static IList<Validations> values()
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

		public static Validations valueOf( string name )
		{
			foreach ( Validations enumInstance in Validations.valueList )
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