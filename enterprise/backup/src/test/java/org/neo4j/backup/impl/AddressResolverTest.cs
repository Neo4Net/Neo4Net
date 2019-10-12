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
namespace Org.Neo4j.backup.impl
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using OptionalHostnamePort = Org.Neo4j.Kernel.impl.util.OptionalHostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class AddressResolverTest
	{

		 internal AddressResolver Subject;

		 // Parameters
		 internal Config DefaultConfig = Config.defaults();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Subject = new AddressResolver();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noPortResolvesToDefault_ha()
		 public virtual void NoPortResolvesToDefaultHa()
		 {
			  // given
			  int? portIsNotSupplied = null;

			  // when
			  HostnamePort resolved = Subject.resolveCorrectHAAddress( DefaultConfig, new OptionalHostnamePort( "localhost", portIsNotSupplied, null ) );

			  // then
			  assertEquals( 6362, resolved.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noPortResolvesToDefault_cc()
		 public virtual void NoPortResolvesToDefaultCc()
		 {
			  Config config = Config.builder().withSetting(OnlineBackupSettings.online_backup_server, "any:1234").build();
			  AdvertisedSocketAddress resolved = Subject.resolveCorrectCCAddress( config, new OptionalHostnamePort( "localhost", null, null ) );

			  // then
			  assertEquals( 1234, resolved.Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void suppliedPortResolvesToGiven_ha()
		 public virtual void SuppliedPortResolvesToGivenHa()
		 {
			  // given
			  int? portIsSupplied = 1234;

			  // when
			  HostnamePort resolved = Subject.resolveCorrectHAAddress( DefaultConfig, new OptionalHostnamePort( "localhost", portIsSupplied, null ) );

			  // then
			  assertEquals( resolved.Port, portIsSupplied.Value );
		 }
	}

}