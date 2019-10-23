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
namespace Neo4Net.Kernel.impl.proc
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using FieldSignature = Neo4Net.Kernel.Api.Internal.procs.FieldSignature;
	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;
	using UserFunctionSignature = Neo4Net.Kernel.Api.Internal.procs.UserFunctionSignature;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.UserFunctionSignature.functionSignature;

	public class UserFunctionSignatureTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();
		 private readonly UserFunctionSignature _signature = functionSignature( "asd" ).@in( "in", Neo4NetTypes.NTAny ).@out( Neo4NetTypes.NTAny ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void inputSignatureShouldNotBeModifiable()
		 public virtual void InputSignatureShouldNotBeModifiable()
		 {
			  // Expect
			  Exception.expect( typeof( System.NotSupportedException ) );

			  // When
			  _signature.inputSignature().Add(FieldSignature.inputField("in2", Neo4NetTypes.NTAny));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toStringShouldMatchCypherSyntax()
		 public virtual void ToStringShouldMatchCypherSyntax()
		 {
			  // When
			  string toStr = functionSignature( "org", "myProcedure" ).@in( "in", Neo4NetTypes.NTList( Neo4NetTypes.NTString ) ).@out( Neo4NetTypes.NTNumber ).build().ToString();

			  // Then
			  assertEquals( "org.myProcedure(in :: LIST? OF STRING?) :: (NUMBER?)", toStr );
		 }
	}

}