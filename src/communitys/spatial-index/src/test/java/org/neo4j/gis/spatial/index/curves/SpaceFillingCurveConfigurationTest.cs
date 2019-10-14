using System;

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
namespace Neo4Net.Gis.Spatial.Index.curves
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;

	internal class SpaceFillingCurveConfigurationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMaxDepthWithEmptySearchArea()
		 internal virtual void ShouldHandleMaxDepthWithEmptySearchArea()
		 {
			  SpaceFillingCurveConfiguration standardConfiguration = new StandardConfiguration();
			  SpaceFillingCurveConfiguration partialOverlapConf = new PartialOverlapConfiguration();
			  // search area is a line, thus having a search area = 0
			  Envelope search = new Envelope( -180, -180, -90, 90 );
			  Envelope range = new Envelope( -180, 180, -90, 90 );
			  // We pad the line to a small area, but we don't expect to go deeper than level 20
			  // which would take too long
			  int maxLevel = 20;
			  assertThat( partialOverlapConf.MaxDepth( search, range, 2, 30 ), lessThan( maxLevel ) );
			  assertThat( standardConfiguration.MaxDepth( search, range, 2, 30 ), lessThan( maxLevel ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnMaxDepth1WithWholeSearchArea()
		 internal virtual void ShouldReturnMaxDepth1WithWholeSearchArea()
		 {
			  SpaceFillingCurveConfiguration standardConfiguration = new StandardConfiguration();
			  SpaceFillingCurveConfiguration partialOverlapConf = new PartialOverlapConfiguration();
			  // search area is a line, thus having a search area = 0
			  Envelope range = new Envelope( -180, 180, -90, 90 );
			  assertThat( partialOverlapConf.MaxDepth( range, range, 2, 30 ), equalTo( 1 ) );
			  assertThat( standardConfiguration.MaxDepth( range, range, 2, 30 ), equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnMaxDepth2WithQuarterOfWholeArea()
		 internal virtual void ShouldReturnMaxDepth2WithQuarterOfWholeArea()
		 {
			  SpaceFillingCurveConfiguration standardConfiguration = new StandardConfiguration();
			  SpaceFillingCurveConfiguration partialOverlapConf = new PartialOverlapConfiguration();
			  // search area is a line, thus having a search area = 0
			  Envelope range = new Envelope( -180, 180, -90, 90 );
			  Envelope search = new Envelope( 0, 180, 0, 90 );
			  assertThat( partialOverlapConf.MaxDepth( search, range, 2, 30 ), equalTo( 2 ) );
			  assertThat( standardConfiguration.MaxDepth( search, range, 2, 30 ), equalTo( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnAppropriateDepth()
		 internal virtual void ShouldReturnAppropriateDepth()
		 {
			  const int maxLevel = 30;
			  for ( int i = 0; i < maxLevel; i++ )
			  {
					SpaceFillingCurveConfiguration standardConfiguration = new StandardConfiguration();
					SpaceFillingCurveConfiguration partialOverlapConf = new PartialOverlapConfiguration();
					// search area is a line, thus having a search area = 0
					Envelope range = new Envelope( 0, 1, 0, 1 );
					Envelope search = new Envelope( 0, Math.Pow( 2, -i ), 0, Math.Pow( 2, -i ) );
					assertThat( partialOverlapConf.MaxDepth( search, range, 2, maxLevel ), equalTo( i + 1 ) );
					assertThat( standardConfiguration.MaxDepth( search, range, 2, maxLevel ), equalTo( i + 1 ) );
			  }
		 }
	}

}