﻿/*
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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Test = org.junit.Test;

	using IndexProgressor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class IndexCursorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClosePreviousBeforeReinitialize()
		 public virtual void ShouldClosePreviousBeforeReinitialize()
		 {
			  // given
			  StubIndexCursor cursor = new StubIndexCursor();
			  StubProgressor progressor = new StubProgressor();
			  cursor.Initialize( progressor );
			  assertFalse( "open before re-initialize", progressor.IsClosed );

			  // when
			  StubProgressor otherProgressor = new StubProgressor();
			  cursor.Initialize( otherProgressor );

			  // then
			  assertTrue( "closed after re-initialize", progressor.IsClosed );
			  assertFalse( "new still open", otherProgressor.IsClosed );
		 }

		 private class StubIndexCursor : IndexCursor<StubProgressor>
		 {
		 }

		 private class StubProgressor : IndexProgressor
		 {
			  internal bool IsClosed;

			  internal StubProgressor()
			  {
					IsClosed = false;
			  }

			  public override bool Next()
			  {
					return false;
			  }

			  public override void Close()
			  {
					IsClosed = true;
			  }
		 }
	}

}