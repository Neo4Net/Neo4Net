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
namespace Neo4Net.causalclustering.core.state.machines.id
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using Test = org.junit.Test;

	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;

	public class FreeIdFilteredIdGeneratorTest
	{

		 private IdGenerator _idGenerator = mock( typeof( IdGenerator ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void freeIdIfConditionSatisfied()
		 public virtual void FreeIdIfConditionSatisfied()
		 {
			  FreeIdFilteredIdGenerator generator = CreateFilteredIdGenerator( _idGenerator, () => true );
			  generator.FreeId( 1 );

			  verify( _idGenerator ).freeId( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipFreeIdIfConditionIsNotSatisfied()
		 public virtual void SkipFreeIdIfConditionIsNotSatisfied()
		 {
			  FreeIdFilteredIdGenerator generator = CreateFilteredIdGenerator( _idGenerator, () => false );
			  generator.FreeId( 1 );

			  verifyZeroInteractions( _idGenerator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void freeIdOnlyWhenConditionSatisfied()
		 public virtual void FreeIdOnlyWhenConditionSatisfied()
		 {
			  MutableBoolean condition = new MutableBoolean();
			  FreeIdFilteredIdGenerator generator = CreateFilteredIdGenerator( _idGenerator, condition.booleanValue );
			  generator.FreeId( 1 );
			  condition.setTrue();
			  generator.FreeId( 2 );

			  verify( _idGenerator ).freeId( 2 );
		 }

		 private FreeIdFilteredIdGenerator CreateFilteredIdGenerator( IdGenerator idGenerator, System.Func<bool> booleanSupplier )
		 {
			  return new FreeIdFilteredIdGenerator( idGenerator, booleanSupplier );
		 }
	}

}