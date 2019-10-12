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
namespace Org.Neo4j.Kernel.ha.cluster.modeswitch
{

	/// <summary>
	/// Container of <seealso cref="ComponentSwitcher"/>s that switches all contained components to master, slave or pending mode.
	/// Assumed to be populated only during startup and is not thread safe.
	/// </summary>
	public class ComponentSwitcherContainer : ComponentSwitcher
	{
		 // Modified during database startup while holding lock on the top level lifecycle instance
		 private readonly IList<ComponentSwitcher> _componentSwitchers = new List<ComponentSwitcher>();

		 public virtual void Add( ComponentSwitcher componentSwitcher )
		 {
			  _componentSwitchers.Add( componentSwitcher );
		 }

		 public override void SwitchToMaster()
		 {
			  _componentSwitchers.ForEach( ComponentSwitcher.switchToMaster );
		 }

		 public override void SwitchToSlave()
		 {
			  _componentSwitchers.ForEach( ComponentSwitcher.switchToSlave );
		 }

		 public override void SwitchToPending()
		 {
			  _componentSwitchers.ForEach( ComponentSwitcher.switchToPending );
		 }

		 public override string ToString()
		 {
			  return "ComponentSwitcherContainer{componentSwitchers=" + _componentSwitchers + "}";
		 }
	}

}