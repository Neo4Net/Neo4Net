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
namespace Org.Neo4j
{
	using Test = org.junit.Test;


	using FormatFamily = Org.Neo4j.Kernel.impl.store.format.FormatFamily;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using StoreVersion = Org.Neo4j.Kernel.impl.store.format.StoreVersion;
	using HighLimit = Org.Neo4j.Kernel.impl.store.format.highlimit.HighLimit;
	using HighLimitV3_0_0 = Org.Neo4j.Kernel.impl.store.format.highlimit.v300.HighLimitV3_0_0;
	using StandardV2_3 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_0 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_0;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class RecordFormatsGenerationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void correctGenerations()
		 public virtual void CorrectGenerations()
		 {
			  IList<RecordFormats> recordFormats = Arrays.asList( StandardV2_3.RECORD_FORMATS, StandardV3_0.RECORD_FORMATS, HighLimitV3_0_0.RECORD_FORMATS, HighLimit.RECORD_FORMATS );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  IDictionary<FormatFamily, IList<int>> generationsForFamilies = recordFormats.collect( groupingBy( RecordFormats::getFormatFamily, mapping( RecordFormats::generation, toList() ) ) );
			  assertEquals( 2, generationsForFamilies.Count );
			  foreach ( KeyValuePair<FormatFamily, IList<int>> familyListGeneration in generationsForFamilies.SetOfKeyValuePairs() )
			  {
					assertEquals( "Generation inside format family should be unique.", familyListGeneration.Value, Distinct( familyListGeneration.Value ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uniqueGenerations()
		 public virtual void UniqueGenerations()
		 {
			  IDictionary<FormatFamily, IList<int>> familyGenerations = AllFamilyGenerations();
			  foreach ( KeyValuePair<FormatFamily, IList<int>> familyEntry in familyGenerations.SetOfKeyValuePairs() )
			  {
					assertEquals( "Generation inside format family should be unique.", familyEntry.Value, Distinct( familyEntry.Value ) );
			  }
		 }

		 private static IDictionary<FormatFamily, IList<int>> AllFamilyGenerations()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return java.util.org.neo4j.kernel.impl.store.format.StoreVersion.values().Select(StoreVersion::versionString).Select(RecordFormatSelector.selectForVersion).collect(groupingBy(RecordFormats::getFormatFamily, mapping(RecordFormats::generation, toList())));
		 }

		 private static IList<int> Distinct( IList<int> integers )
		 {
			  return integers.Distinct().ToList();
		 }
	}

}