using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{

	/// <summary>
	/// Simple implementation of a trigger info taking in construction the name/description of what triggered the check point
	/// and offering the possibility to be enriched with a single optional extra description.
	/// </summary>
	public class SimpleTriggerInfo : TriggerInfo
	{
		 private readonly string _triggerName;
		 private string _description;

		 public SimpleTriggerInfo( string triggerName )
		 {
			  Debug.Assert( !string.ReferenceEquals( triggerName, null ) );
			  this._triggerName = triggerName;
		 }

		 public override string Describe( long transactionId )
		 {
			  string info = string.ReferenceEquals( _description, null ) ? _triggerName : _triggerName + " for " + _description;
			  return "Checkpoint triggered by \"" + info + "\" @ txId: " + transactionId;
		 }

		 public override void Accept( string description )
		 {
			  Debug.Assert( !string.ReferenceEquals( description, null ) );
			  Debug.Assert( string.ReferenceEquals( this._description, null ) );
			  this._description = description;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  SimpleTriggerInfo that = ( SimpleTriggerInfo ) o;
			  return Objects.Equals( _triggerName, that._triggerName ) && Objects.Equals( _description, that._description );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _triggerName, _description );
		 }
	}

}