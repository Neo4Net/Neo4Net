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
namespace Neo4Net.management
{
	using Description = Neo4Net.Jmx.Description;
	using ManagementInterface = Neo4Net.Jmx.ManagementInterface;

	[ManagementInterface(name : IndexSamplingManager_Fields.NAME), Description("Handle index sampling.")]
	public interface IndexSamplingManager
	{

		 [Description("Trigger index sampling for the index associated with the provided label and property key." + " If forceSample is set to true an index sampling will always happen otherwise a sampling is only " + "done if the number of updates exceeds the configured dbms.index_sampling.update_percentage.")]
		 void TriggerIndexSampling( string labelKey, string propertyKey, bool forceSample );
	}

	public static class IndexSamplingManager_Fields
	{
		 public const string NAME = "Index sampler";
	}

}