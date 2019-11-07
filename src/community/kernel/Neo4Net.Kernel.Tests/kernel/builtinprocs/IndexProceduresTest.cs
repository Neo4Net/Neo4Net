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
namespace Neo4Net.Kernel.builtinprocs
{
	using Test = org.junit.Test;

	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using Statement = Neo4Net.Kernel.Api.Statement;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class IndexProceduresTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeStatementOnClose()
		 public virtual void CloseStatementOnClose()
		 {
			  KernelTransaction kernelTransaction = mock( typeof( KernelTransaction ) );
			  Statement statement = mock( typeof( Statement ) );
			  when( kernelTransaction.AcquireStatement() ).thenReturn(statement);
			  //noinspection EmptyTryBlock
			  using ( IndexProcedures ignored = new IndexProcedures( kernelTransaction, null ) )
			  {
			  }
			  verify( statement ).close();
		 }
	}

}