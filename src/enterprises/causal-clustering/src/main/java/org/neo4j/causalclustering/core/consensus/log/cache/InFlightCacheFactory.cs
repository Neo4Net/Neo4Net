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
namespace Neo4Net.causalclustering.core.consensus.log.cache
{
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.in_flight_cache_max_bytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.in_flight_cache_max_entries;

	public class InFlightCacheFactory
	{
		 public static InFlightCache Create( Config config, Monitors monitors )
		 {
			  return config.Get( CausalClusteringSettings.in_flight_cache_type ).create( config, monitors );
		 }

		 public abstract class Type
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NONE { InFlightCache create(org.neo4j.kernel.configuration.Config config, org.neo4j.kernel.monitoring.Monitors monitors) { return new VoidInFlightCache(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           CONSECUTIVE { InFlightCache create(org.neo4j.kernel.configuration.Config config, org.neo4j.kernel.monitoring.Monitors monitors) { return new ConsecutiveInFlightCache(config.get(in_flight_cache_max_entries), config.get(in_flight_cache_max_bytes), monitors.newMonitor(InFlightCacheMonitor.class), false); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           UNBOUNDED { InFlightCache create(org.neo4j.kernel.configuration.Config config, org.neo4j.kernel.monitoring.Monitors monitors) { return new UnboundedInFlightCache(); } };

			  private static readonly IList<Type> valueList = new List<Type>();

			  static Type()
			  {
				  valueList.Add( NONE );
				  valueList.Add( CONSECUTIVE );
				  valueList.Add( UNBOUNDED );
			  }

			  public enum InnerEnum
			  {
				  NONE,
				  CONSECUTIVE,
				  UNBOUNDED
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Type( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract InFlightCache create( Neo4Net.Kernel.configuration.Config config, Neo4Net.Kernel.monitoring.Monitors monitors );

			 public static IList<Type> values()
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

			 public static Type valueOf( string name )
			 {
				 foreach ( Type enumInstance in Type.valueList )
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

}