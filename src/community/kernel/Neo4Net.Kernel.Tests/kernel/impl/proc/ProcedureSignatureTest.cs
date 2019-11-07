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
	using ProcedureSignature = Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature.procedureSignature;

	public class ProcedureSignatureTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();
		 private readonly ProcedureSignature _signature = procedureSignature( "asd" ).@in( "a", Neo4NetTypes.NTAny ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void inputSignatureShouldNotBeModifiable()
		 public virtual void InputSignatureShouldNotBeModifiable()
		 {
			  // Expect
			  Exception.expect( typeof( System.NotSupportedException ) );

			  // When
			  _signature.inputSignature().Add(FieldSignature.inputField("b", Neo4NetTypes.NTAny));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void outputSignatureShouldNotBeModifiable()
		 public virtual void OutputSignatureShouldNotBeModifiable()
		 {
			  // Expect
			  Exception.expect( typeof( System.NotSupportedException ) );

			  // When
			  _signature.outputSignature().Add(FieldSignature.outputField("b", Neo4NetTypes.NTAny));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHonorVoidInEquals()
		 public virtual void ShouldHonorVoidInEquals()
		 {
			  ProcedureSignature sig1 = procedureSignature( "foo" ).@in( "a", Neo4NetTypes.NTAny ).build();
			  ProcedureSignature sig2 = procedureSignature( "foo" ).@in( "a", Neo4NetTypes.NTAny ).@out( ProcedureSignature.VOID ).build();
			  ProcedureSignature sig2clone = procedureSignature( "foo" ).@in( "a", Neo4NetTypes.NTAny ).@out( ProcedureSignature.VOID ).build();

			  assertEquals( sig2, sig2clone );
			  assertNotEquals( sig1, sig2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toStringShouldMatchCypherSyntax()
		 public virtual void ToStringShouldMatchCypherSyntax()
		 {
			  // When
			  string toStr = procedureSignature( "org", "myProcedure" ).@in( "inputArg", Neo4NetTypes.NTList( Neo4NetTypes.NTString ) ).@out( "outputArg", Neo4NetTypes.NTNumber ).build().ToString();

			  // Then
			  assertEquals( "org.myProcedure(inputArg :: LIST? OF STRING?) :: (outputArg :: NUMBER?)", toStr );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toStringForVoidProcedureShouldMatchCypherSyntax()
		 public virtual void ToStringForVoidProcedureShouldMatchCypherSyntax()
		 {
			  // Given
			  ProcedureSignature proc = procedureSignature( "org", "myProcedure" ).@in( "inputArg", Neo4NetTypes.NTList( Neo4NetTypes.NTString ) ).@out( ProcedureSignature.VOID ).build();

			  // When
			  string toStr = proc.ToString();

			  // Then
			  assertEquals( "org.myProcedure(inputArg :: LIST? OF STRING?) :: VOID", toStr );
		 }
	}

}