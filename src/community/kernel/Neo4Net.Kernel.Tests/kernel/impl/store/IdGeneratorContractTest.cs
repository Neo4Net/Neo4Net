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
namespace Neo4Net.Kernel.impl.store
{
	using Test = org.junit.Test;

	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public abstract class IdGeneratorContractTest
	{
		 protected internal abstract IdGenerator CreateIdGenerator( int grabSize );

		 protected internal abstract IdGenerator OpenIdGenerator( int grabSize );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCorrectHighId()
		 public virtual void ShouldReportCorrectHighId()
		 {
			  // given
			  using ( IdGenerator idGenerator = CreateIdGenerator( 2 ) )
			  {
					assertEquals( 0, idGenerator.HighId );
					assertEquals( -1, idGenerator.HighestPossibleIdInUse );

					// when
					idGenerator.NextId();

					// then
					assertEquals( 1, idGenerator.HighId );
					assertEquals( 0, idGenerator.HighestPossibleIdInUse );
			  }
		 }

	}

}