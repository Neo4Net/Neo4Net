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
namespace Org.Neo4j.causalclustering.stresstests
{
	using Workload = Org.Neo4j.helper.Workload;

	internal abstract class Workloads
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       CreateNodesWithProperties { Workload create(Control control, Resources resources, Config config) { return new CreateNodesWithProperties(control, resources, config); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       StartStopRandomMember { Workload create(Control control, Resources resources, Config config) { return new StartStopRandomMember(control, resources); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       StartStopRandomCore { Workload create(Control control, Resources resources, Config config) { return new StartStopRandomCore(control, resources); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       BackupRandomMember { Workload create(Control control, Resources resources, Config config) { return new BackupRandomMember(control, resources); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       CatchupNewReadReplica { Workload create(Control control, Resources resources, Config config) { return new CatchupNewReadReplica(control, resources); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ReplaceRandomMember { Workload create(Control control, Resources resources, Config config) { return new ReplaceRandomMember(control, resources); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       IdReuseInsertion { Workload create(Control control, Resources resources, Config config) { return new IdReuse.InsertionWorkload(control, resources); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       IdReuseDeletion { Workload create(Control control, Resources resources, Config config) { return new IdReuse.DeletionWorkload(control, resources); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       IdReuseReelection { Workload create(Control control, Resources resources, Config config) { return new IdReuse.ReelectionWorkload(control, resources, config); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       FailingWorkload { Workload create(Control control, Resources resources, Config config) { return new FailingWorkload(control); } };

		 private static readonly IList<Workloads> valueList = new List<Workloads>();

		 static Workloads()
		 {
			 valueList.Add( CreateNodesWithProperties );
			 valueList.Add( StartStopRandomMember );
			 valueList.Add( StartStopRandomCore );
			 valueList.Add( BackupRandomMember );
			 valueList.Add( CatchupNewReadReplica );
			 valueList.Add( ReplaceRandomMember );
			 valueList.Add( IdReuseInsertion );
			 valueList.Add( IdReuseDeletion );
			 valueList.Add( IdReuseReelection );
			 valueList.Add( FailingWorkload );
		 }

		 public enum InnerEnum
		 {
			 CreateNodesWithProperties,
			 StartStopRandomMember,
			 StartStopRandomCore,
			 BackupRandomMember,
			 CatchupNewReadReplica,
			 ReplaceRandomMember,
			 IdReuseInsertion,
			 IdReuseDeletion,
			 IdReuseReelection,
			 FailingWorkload
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private Workloads( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal abstract Org.Neo4j.helper.Workload create( Control control, Resources resources, Config config );

		public static IList<Workloads> values()
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

		public static Workloads valueOf( string name )
		{
			foreach ( Workloads enumInstance in Workloads.valueList )
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