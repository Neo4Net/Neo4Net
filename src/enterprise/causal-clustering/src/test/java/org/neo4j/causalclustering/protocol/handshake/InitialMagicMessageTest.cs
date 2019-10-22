using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.protocol.handshake
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class InitialMagicMessageTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateWithCorrectMagicValue()
		 public virtual void ShouldCreateWithCorrectMagicValue()
		 {
			  // given
			  InitialMagicMessage magicMessage = InitialMagicMessage.Instance();

			  // then
			  assertTrue( magicMessage.CorrectMagic );
			  assertEquals( "Neo4Net_CLUSTER", magicMessage.Magic() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveCorrectMessageCode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveCorrectMessageCode()
		 {
			  sbyte[] bytes = InitialMagicMessage.CORRECT_MAGIC_VALUE.Substring( 0, 4 ).GetBytes( Encoding.UTF8 );
			  int messageCode = bytes[0] | ( bytes[1] << 8 ) | ( bytes[2] << 16 ) | ( bytes[3] << 24 );

			  assertEquals( 0x344F454E, messageCode );
			  assertEquals( 0x344F454E, InitialMagicMessage.MESSAGE_CODE );
		 }
	}

}