using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.ha.cluster.member
{

	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using Cluster = Org.Neo4j.cluster.protocol.cluster.Cluster;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Org.Neo4j.cluster.protocol.cluster.ClusterListener;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using HighAvailabilityModeSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using Slave = Org.Neo4j.Kernel.ha.com.master.Slave;
	using SlaveFactory = Org.Neo4j.Kernel.ha.com.master.SlaveFactory;
	using Slaves = Org.Neo4j.Kernel.ha.com.master.Slaves;
	using Org.Neo4j.Kernel.impl.util;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.member.ClusterMembers.inRole;

	/// <summary>
	/// Keeps active connections to <seealso cref="Slave slaves"/> for a master to communicate to
	/// when so needed.
	/// </summary>
	public class HighAvailabilitySlaves : Lifecycle, Slaves
	{
		 private readonly LifeSupport _life = new LifeSupport();
		 private readonly IDictionary<ClusterMember, Slave> _slaves = new CopyOnWriteHashMap<ClusterMember, Slave>();
		 private readonly ClusterMembers _clusterMembers;
		 private readonly Cluster _cluster;
		 private readonly SlaveFactory _slaveFactory;
		 private readonly HostnamePort _me;
		 private HighAvailabilitySlaves.HASClusterListener _clusterListener;

		 public HighAvailabilitySlaves( ClusterMembers clusterMembers, Cluster cluster, SlaveFactory slaveFactory, HostnamePort me )
		 {
			  this._clusterMembers = clusterMembers;
			  this._cluster = cluster;
			  this._slaveFactory = slaveFactory;
			  this._me = me;
		 }

		 private System.Func<ClusterMember, Slave> SlaveForMember()
		 {
			  return from =>
			  {
				lock ( HighAvailabilitySlaves.this )
				{
					 return _slaves.computeIfAbsent( from, f => _slaveFactory.newSlave( _life, f, _me.Host, _me.Port ) );
				}
			  };
		 }

		 public virtual IEnumerable<Slave> Slaves
		 {
			 get
			 {
				  // Return all cluster members which are currently SLAVEs,
				  // are alive, and convert to Slave with a cache if possible
				  return map(clusterMember =>
				  {
					Slave slave = SlaveForMember().apply(clusterMember);
   
					if ( slave == null )
					{
						 return _slaves[clusterMember];
					}
					else
					{
						 return slave;
					}
				  }, filter( inRole( HighAvailabilityModeSwitcher.SLAVE ), _clusterMembers.AliveMembers ));
			 }
		 }

		 public override void Init()
		 {
			  _life.init();

			  _clusterListener = new HASClusterListener( this );
			  _cluster.addClusterListener( _clusterListener );
		 }

		 public override void Start()
		 {
			  _life.start();
		 }

		 public override void Stop()
		 {
			  _life.stop();
		 }

		 public override void Shutdown()
		 {
			  _cluster.removeClusterListener( _clusterListener );

			  _life.shutdown();
			  _slaves.Clear();
		 }

		 private class HASClusterListener : Org.Neo4j.cluster.protocol.cluster.ClusterListener_Adapter
		 {
			 private readonly HighAvailabilitySlaves _outerInstance;

			 public HASClusterListener( HighAvailabilitySlaves outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Elected( string role, InstanceId instanceId, URI electedMember )
			  {
					if ( role.Equals( ClusterConfiguration.COORDINATOR ) )
					{
						 outerInstance.life.Clear();
						 outerInstance.slaves.Clear();
					}
			  }
		 }
	}

}