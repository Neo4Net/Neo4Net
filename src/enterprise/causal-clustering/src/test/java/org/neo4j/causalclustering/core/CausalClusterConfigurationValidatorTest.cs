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
namespace Neo4Net.causalclustering.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using InvalidSettingException = Neo4Net.GraphDb.config.InvalidSettingException;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using Mode = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings.Mode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.discovery_type;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.initial_discovery_members;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.kubernetes_label_selector;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.kubernetes_service_port_name;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class CausalClusterConfigurationValidatorTest
	public class CausalClusterConfigurationValidatorTest
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
			  return Arrays.asList( EnterpriseEditionSettings.Mode.CORE, EnterpriseEditionSettings.Mode.READ_REPLICA );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateOnlyIfModeIsCoreOrReplica()
		 public virtual void ValidateOnlyIfModeIsCoreOrReplica()
		 {
			  // when
			  Config config = Config.builder().withSetting(EnterpriseEditionSettings.mode, EnterpriseEditionSettings.Mode.SINGLE.name()).withSetting(initial_discovery_members, "").withValidator(new CausalClusterConfigurationValidator()).build();

			  // then
			  Optional<string> value = config.GetRaw( initial_discovery_members.name() );
			  assertTrue( value.Present );
			  assertEquals( "", value.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateSuccessList()
		 public virtual void ValidateSuccessList()
		 {
			  // when
			  Config config = Config.builder().withSetting(EnterpriseEditionSettings.mode, EnterpriseEditionSettings.Mode.SINGLE.name()).withSetting(initial_discovery_members, "localhost:99,remotehost:2").withSetting((new BoltConnector("bolt")).enabled.name(), "true").withValidator(new CausalClusterConfigurationValidator()).build();

			  // then
			  assertEquals( asList( new AdvertisedSocketAddress( "localhost", 99 ), new AdvertisedSocketAddress( "remotehost", 2 ) ), config.Get( initial_discovery_members ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void validateSuccessKubernetes()
		 public virtual void ValidateSuccessKubernetes()
		 {
			  // when
			  Config.builder().withSetting(EnterpriseEditionSettings.mode, EnterpriseEditionSettings.Mode.SINGLE.name()).withSetting(discovery_type, DiscoveryType.K8s.name()).withSetting(kubernetes_label_selector, "waldo=fred").withSetting(kubernetes_service_port_name, "default").withSetting((new BoltConnector("bolt")).enabled.name(), "true").withValidator(new CausalClusterConfigurationValidator()).build();

			  // then no exception
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingBoltConnector()
		 public virtual void MissingBoltConnector()
		 {
			  // then
			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "A Bolt connector must be configured to run a cluster" );

			  // when
			  Config.builder().withSetting(EnterpriseEditionSettings.mode.name(), Mode.name()).withSetting(initial_discovery_members, "").withSetting(initial_discovery_members.name(), "localhost:99,remotehost:2").withValidator(new CausalClusterConfigurationValidator()).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingInitialMembersDNS()
		 public virtual void MissingInitialMembersDNS()
		 {
			  // then
			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Missing value for 'causal_clustering.initial_discovery_members', which is mandatory with 'causal_clustering.discovery_type=DNS'" );

			  // when
			  Config.builder().withSetting(EnterpriseEditionSettings.mode, Mode.name()).withSetting(discovery_type, DiscoveryType.Dns.name()).withValidator(new CausalClusterConfigurationValidator()).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingInitialMembersLIST()
		 public virtual void MissingInitialMembersLIST()
		 {
			  // then
			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Missing value for 'causal_clustering.initial_discovery_members', which is mandatory with 'causal_clustering.discovery_type=LIST'" );

			  // when
			  Config.builder().withSetting(EnterpriseEditionSettings.mode, Mode.name()).withSetting(discovery_type, DiscoveryType.List.name()).withValidator(new CausalClusterConfigurationValidator()).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingInitialMembersSRV()
		 public virtual void MissingInitialMembersSRV()
		 {
			  // then
			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Missing value for 'causal_clustering.initial_discovery_members', which is mandatory with 'causal_clustering.discovery_type=SRV'" );

			  // when
			  Config.builder().withSetting(EnterpriseEditionSettings.mode, Mode.name()).withSetting(discovery_type, DiscoveryType.Srv.name()).withValidator(new CausalClusterConfigurationValidator()).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingKubernetesLabelSelector()
		 public virtual void MissingKubernetesLabelSelector()
		 {
			  // then
			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Missing value for 'causal_clustering.kubernetes.label_selector', which is mandatory with 'causal_clustering.discovery_type=K8S'" );

			  // when
			  Config.builder().withSetting(EnterpriseEditionSettings.mode, Mode.name()).withSetting(discovery_type, DiscoveryType.K8s.name()).withSetting(kubernetes_service_port_name, "default").withSetting((new BoltConnector("bolt")).enabled.name(), "true").withValidator(new CausalClusterConfigurationValidator()).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void missingKubernetesPortName()
		 public virtual void MissingKubernetesPortName()
		 {
			  // then
			  Expected.expect( typeof( InvalidSettingException ) );
			  Expected.expectMessage( "Missing value for 'causal_clustering.kubernetes.service_port_name', which is mandatory with 'causal_clustering.discovery_type=K8S'" );

			  // when
			  Config.builder().withSetting(EnterpriseEditionSettings.mode, Mode.name()).withSetting(discovery_type, DiscoveryType.K8s.name()).withSetting(kubernetes_label_selector, "waldo=fred").withSetting((new BoltConnector("bolt")).enabled.name(), "true").withValidator(new CausalClusterConfigurationValidator()).build();
		 }
	}

}