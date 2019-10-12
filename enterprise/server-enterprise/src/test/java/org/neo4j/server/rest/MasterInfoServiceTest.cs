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
namespace Org.Neo4j.Server.rest
{
	using Test = org.junit.Test;

	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class MasterInfoServiceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void masterShouldRespond200AndTrueWhenMaster()
		 public virtual void MasterShouldRespond200AndTrueWhenMaster()
		 {
			  // given
			  HighlyAvailableGraphDatabase database = mock( typeof( HighlyAvailableGraphDatabase ) );
			  when( database.Role() ).thenReturn("master");

			  MasterInfoService service = new MasterInfoService( null, database );

			  // when
			  Response response = service.Master;

			  // then
			  assertEquals( 200, response.Status );
			  assertEquals( "true", response.Entity.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void masterShouldRespond404AndFalseWhenSlave()
		 public virtual void MasterShouldRespond404AndFalseWhenSlave()
		 {
			  // given
			  HighlyAvailableGraphDatabase database = mock( typeof( HighlyAvailableGraphDatabase ) );
			  when( database.Role() ).thenReturn("slave");

			  MasterInfoService service = new MasterInfoService( null, database );

			  // when
			  Response response = service.Master;

			  // then
			  assertEquals( 404, response.Status );
			  assertEquals( "false", response.Entity.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void masterShouldRespond404AndUNKNOWNWhenUnknown()
		 public virtual void MasterShouldRespond404AndUNKNOWNWhenUnknown()
		 {
			  // given
			  HighlyAvailableGraphDatabase database = mock( typeof( HighlyAvailableGraphDatabase ) );
			  when( database.Role() ).thenReturn("unknown");

			  MasterInfoService service = new MasterInfoService( null, database );

			  // when
			  Response response = service.Master;

			  // then
			  assertEquals( 404, response.Status );
			  assertEquals( "UNKNOWN", response.Entity.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveShouldRespond200AndTrueWhenSlave()
		 public virtual void SlaveShouldRespond200AndTrueWhenSlave()
		 {
			  // given
			  HighlyAvailableGraphDatabase database = mock( typeof( HighlyAvailableGraphDatabase ) );
			  when( database.Role() ).thenReturn("slave");

			  MasterInfoService service = new MasterInfoService( null, database );

			  // when
			  Response response = service.Slave;

			  // then
			  assertEquals( 200, response.Status );
			  assertEquals( "true", response.Entity.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveShouldRespond404AndFalseWhenMaster()
		 public virtual void SlaveShouldRespond404AndFalseWhenMaster()
		 {
			  // given
			  HighlyAvailableGraphDatabase database = mock( typeof( HighlyAvailableGraphDatabase ) );
			  when( database.Role() ).thenReturn("master");

			  MasterInfoService service = new MasterInfoService( null, database );

			  // when
			  Response response = service.Slave;

			  // then
			  assertEquals( 404, response.Status );
			  assertEquals( "false", response.Entity.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveShouldRespond404AndUNKNOWNWhenUnknown()
		 public virtual void SlaveShouldRespond404AndUNKNOWNWhenUnknown()
		 {
			  // given
			  HighlyAvailableGraphDatabase database = mock( typeof( HighlyAvailableGraphDatabase ) );
			  when( database.Role() ).thenReturn("unknown");

			  MasterInfoService service = new MasterInfoService( null, database );

			  // when
			  Response response = service.Slave;

			  // then
			  assertEquals( 404, response.Status );
			  assertEquals( "UNKNOWN", response.Entity.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportMasterAsGenerallyAvailableForTransactionProcessing()
		 public virtual void ShouldReportMasterAsGenerallyAvailableForTransactionProcessing()
		 {
			  // given
			  HighlyAvailableGraphDatabase database = mock( typeof( HighlyAvailableGraphDatabase ) );
			  when( database.Role() ).thenReturn("master");

			  MasterInfoService service = new MasterInfoService( null, database );

			  // when
			  Response response = service.Available;

			  // then
			  assertEquals( 200, response.Status );
			  assertEquals( "master", response.Entity.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportSlaveAsGenerallyAvailableForTransactionProcessing()
		 public virtual void ShouldReportSlaveAsGenerallyAvailableForTransactionProcessing()
		 {
			  // given
			  HighlyAvailableGraphDatabase database = mock( typeof( HighlyAvailableGraphDatabase ) );
			  when( database.Role() ).thenReturn("slave");

			  MasterInfoService service = new MasterInfoService( null, database );

			  // when
			  Response response = service.Available;

			  // then
			  assertEquals( 200, response.Status );
			  assertEquals( "slave", response.Entity.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNonMasterOrSlaveAsUnavailableForTransactionProcessing()
		 public virtual void ShouldReportNonMasterOrSlaveAsUnavailableForTransactionProcessing()
		 {
			  // given
			  HighlyAvailableGraphDatabase database = mock( typeof( HighlyAvailableGraphDatabase ) );
			  when( database.Role() ).thenReturn("unknown");

			  MasterInfoService service = new MasterInfoService( null, database );

			  // when
			  Response response = service.Available;

			  // then
			  assertEquals( 404, response.Status );
			  assertEquals( "UNKNOWN", response.Entity.ToString() );
		 }
	}

}