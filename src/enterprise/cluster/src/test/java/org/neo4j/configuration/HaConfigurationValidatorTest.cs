using System.Collections.Generic;

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
namespace Neo4Net.Configuration
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using InvalidSettingException = Neo4Net.GraphDb.config.InvalidSettingException;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class HaConfigurationValidatorTest
	public class HaConfigurationValidatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public org.Neo4Net.kernel.impl.enterprise.configuration.EnterpriseEditionSettings.Mode mode;
		 public EnterpriseEditionSettings.Mode Mode;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.List<org.Neo4Net.kernel.impl.enterprise.configuration.EnterpriseEditionSettings.Mode> recordFormats()
		 public static IList<EnterpriseEditionSettings.Mode> RecordFormats()
		 {
			  return Arrays.asList( EnterpriseEditionSettings.Mode.HA, EnterpriseEditionSettings.Mode.ARBITER );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateOnlyIfModeIsHA()
		 public virtual void ValidateOnlyIfModeIsHA()
		 {
			  // when
			  Config config = Config.fromSettings( stringMap( EnterpriseEditionSettings.mode.name(), EnterpriseEditionSettings.Mode.SINGLE.name(), ClusterSettings.initial_hosts.name(), "" ) ).withValidator(new HaConfigurationValidator()).build();

			  // then
			  Optional<string> value = config.GetRaw( ClusterSettings.initial_hosts.name() );
			  assertTrue( value.Present );
			  assertEquals( "", value.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateSuccess()
		 public virtual void ValidateSuccess()
		 {
			  // when
			  Config config = Config.fromSettings( stringMap( EnterpriseEditionSettings.mode.name(), Mode.name(), ClusterSettings.server_id.name(), "1", ClusterSettings.initial_hosts.name(), "localhost,remotehost" ) ).withValidator(new HaConfigurationValidator()).build();

			  // then
			  assertEquals( asList( new HostnamePort( "localhost" ), new HostnamePort( "remotehost" ) ), config.Get( ClusterSettings.initial_hosts ) );
			  assertEquals( new InstanceId( 1 ), config.Get( ClusterSettings.server_id ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingServerId()
		 public virtual void MissingServerId()
		 {
			  // then
			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Missing mandatory value for 'ha.server_id'" );

			  // when
			  Config.fromSettings( stringMap( EnterpriseEditionSettings.mode.name(), Mode.name() ) ).withValidator(new HaConfigurationValidator()).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingInitialHosts()
		 public virtual void MissingInitialHosts()
		 {
			  // then
			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Missing mandatory non-empty value for 'ha.initial_hosts'" );

			  // when
			  Config.fromSettings( stringMap( EnterpriseEditionSettings.mode.name(), Mode.name(), ClusterSettings.server_id.name(), "1" ) ).withValidator(new HaConfigurationValidator()).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void initialHostsEmpty()
		 public virtual void InitialHostsEmpty()
		 {
			  // then
			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Missing mandatory non-empty value for 'ha.initial_hosts'" );

			  // when
			  Config.fromSettings( stringMap( EnterpriseEditionSettings.mode.name(), Mode.name(), ClusterSettings.server_id.name(), "1", ClusterSettings.initial_hosts.name(), "," ) ).withValidator(new HaConfigurationValidator()).build();
		 }
	}

}