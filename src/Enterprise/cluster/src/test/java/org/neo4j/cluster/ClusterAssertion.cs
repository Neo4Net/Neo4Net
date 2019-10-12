using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.cluster
{

	public class ClusterAssertion
	{
		 protected internal IDictionary<int, InstanceAssertion> In;
		 protected internal IDictionary<int, InstanceAssertion> Out;

		 private ClusterAssertion()
		 {
		 }

		 public static ClusterAssertion BasedOn( int[] serverIds )
		 {
			  IDictionary<int, InstanceAssertion> @out = new Dictionary<int, InstanceAssertion>();

			  for ( int i = 0; i < serverIds.Length; i++ )
			  {
					@out[i + 1] = new InstanceAssertion( serverIds[i], URI.create( "cluster://server" + serverIds[i] ) );
			  }

			  ClusterAssertion result = new ClusterAssertion();
			  result.In = new Dictionary<int, InstanceAssertion>();
			  result.Out = @out;
			  return result;
		 }

		 protected internal virtual void Copy( ClusterAssertion assertion )
		 {
			  this.In = assertion.In;
			  this.Out = assertion.Out;
		 }

		 public virtual ClusterAssertion Joins( params int[] joiners )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<int, InstanceAssertion> newIn = new java.util.HashMap<>(in);
			  IDictionary<int, InstanceAssertion> newIn = new Dictionary<int, InstanceAssertion>( In );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<int, InstanceAssertion> newOut = new java.util.HashMap<>(out);
			  IDictionary<int, InstanceAssertion> newOut = new Dictionary<int, InstanceAssertion>( Out );
			  foreach ( int joiner in joiners )
			  {
					newIn[joiner] = newOut.Remove( joiner );
			  }
			  return new ClusterAssertionAnonymousInnerClass( this, newIn, newOut );
		 }

		 private class ClusterAssertionAnonymousInnerClass : ClusterAssertion
		 {
			 private readonly ClusterAssertion _outerInstance;

			 private IDictionary<int, InstanceAssertion> _newIn;
			 private IDictionary<int, InstanceAssertion> _newOut;

			 public ClusterAssertionAnonymousInnerClass( ClusterAssertion outerInstance, IDictionary<int, InstanceAssertion> newIn, IDictionary<int, InstanceAssertion> newOut )
			 {
				 this.outerInstance = outerInstance;
				 this._newIn = newIn;
				 this._newOut = newOut;

				 this.@in = newIn;
				 this.@out = newOut;
			 }

		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ClusterAssertion failed(final int failed)
		 public virtual ClusterAssertion Failed( int failed )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<int, InstanceAssertion> newIn = new java.util.HashMap<>(in);
			  IDictionary<int, InstanceAssertion> newIn = new Dictionary<int, InstanceAssertion>( In );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final InstanceAssertion current = in.get(failed);
			  InstanceAssertion current = In[failed];

			  InstanceAssertion failedInstance = new InstanceAssertionAnonymousInnerClass( this, current );

			  newIn[failed] = failedInstance;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ClusterAssertion realThis = this;
			  ClusterAssertion realThis = this;
			  return new ClusterAssertionAnonymousInnerClass2( this, newIn, realThis );
		 }

		 private class InstanceAssertionAnonymousInnerClass : InstanceAssertion
		 {
			 private readonly ClusterAssertion _outerInstance;

			 private Neo4Net.cluster.ClusterAssertion.InstanceAssertion _current;

			 public InstanceAssertionAnonymousInnerClass( ClusterAssertion outerInstance, Neo4Net.cluster.ClusterAssertion.InstanceAssertion current )
			 {
				 this.outerInstance = outerInstance;
				 this._current = current;

				 outerInstance.Copy( current );
				 this.isFailed = true;
			 }

		 }

		 private class ClusterAssertionAnonymousInnerClass2 : ClusterAssertion
		 {
			 private readonly ClusterAssertion _outerInstance;

			 private IDictionary<int, InstanceAssertion> _newIn;
			 private Neo4Net.cluster.ClusterAssertion _realThis;

			 public ClusterAssertionAnonymousInnerClass2( ClusterAssertion outerInstance, IDictionary<int, InstanceAssertion> newIn, Neo4Net.cluster.ClusterAssertion realThis )
			 {
				 this.outerInstance = outerInstance;
				 this._newIn = newIn;
				 this._realThis = realThis;

				 outerInstance.Copy( realThis );
				 this.@in = newIn;
			 }

		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ClusterAssertion elected(final int elected, final String atRole)
		 public virtual ClusterAssertion Elected( int elected, string atRole )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<int, InstanceAssertion> newIn = new java.util.HashMap<>();
			  IDictionary<int, InstanceAssertion> newIn = new Dictionary<int, InstanceAssertion>();
			  foreach ( KeyValuePair<int, InstanceAssertion> instanceAssertionEntry in In.SetOfKeyValuePairs() )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final InstanceAssertion assertion = instanceAssertionEntry.getValue();
					InstanceAssertion assertion = instanceAssertionEntry.Value;
					if ( !instanceAssertionEntry.Value.isFailed )
					{
						 newIn[instanceAssertionEntry.Key] = new InstanceAssertionAnonymousInnerClass2( this, elected, atRole, assertion );
					}
					else
					{
						 newIn[instanceAssertionEntry.Key] = instanceAssertionEntry.Value;
					}
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ClusterAssertion realThis = this;
			  ClusterAssertion realThis = this;
			  return new ClusterAssertionAnonymousInnerClass3( this, newIn, realThis );
		 }

		 private class InstanceAssertionAnonymousInnerClass2 : InstanceAssertion
		 {
			 private readonly ClusterAssertion _outerInstance;

			 private int _elected;
			 private string _atRole;
			 private Neo4Net.cluster.ClusterAssertion.InstanceAssertion _assertion;

			 public InstanceAssertionAnonymousInnerClass2( ClusterAssertion outerInstance, int elected, string atRole, Neo4Net.cluster.ClusterAssertion.InstanceAssertion assertion )
			 {
				 this.outerInstance = outerInstance;
				 this._elected = elected;
				 this._atRole = atRole;
				 this._assertion = assertion;

				 outerInstance.Copy( assertion );
				 this.roles.put( atRole, elected );
			 }

		 }

		 private class ClusterAssertionAnonymousInnerClass3 : ClusterAssertion
		 {
			 private readonly ClusterAssertion _outerInstance;

			 private IDictionary<int, InstanceAssertion> _newIn;
			 private Neo4Net.cluster.ClusterAssertion _realThis;

			 public ClusterAssertionAnonymousInnerClass3( ClusterAssertion outerInstance, IDictionary<int, InstanceAssertion> newIn, Neo4Net.cluster.ClusterAssertion realThis )
			 {
				 this.outerInstance = outerInstance;
				 this._newIn = newIn;
				 this._realThis = realThis;

				 outerInstance.Copy( realThis );
				 this.@in = newIn;
			 }

		 }

		 public virtual InstanceAssertion[] Snapshot()
		 {
			  InstanceAssertion[] result = new InstanceAssertion[In.Count + Out.Count];
			  foreach ( KeyValuePair<int, InstanceAssertion> inEntry in In.SetOfKeyValuePairs() )
			  {
					int key = inEntry.Key - 1;
					Debug.Assert( result[key] == null, "double entry for " + inEntry.Key );
					result[key] = inEntry.Value;
			  }
			  foreach ( KeyValuePair<int, InstanceAssertion> outEntry in Out.SetOfKeyValuePairs() )
			  {
					int key = outEntry.Key - 1;
					Debug.Assert( result[key] == null, "double entry for " + outEntry.Key );
					result[key] = outEntry.Value;
			  }
			  return result;
		 }

		 public class InstanceAssertion
		 {
			  internal int ServerId;
			  internal URI Uri;
			  internal bool IsFailed;
			  internal IDictionary<int, URI> ReachableInstances = new Dictionary<int, URI>();
			  internal IDictionary<int, URI> FailedInstances = new Dictionary<int, URI>();
			  internal IDictionary<string, int> Roles = new Dictionary<string, int>();

			  internal InstanceAssertion()
			  {
			  }

			  internal InstanceAssertion( int serverId, URI uri )
			  {
					this.ServerId = serverId;
					this.Uri = uri;
			  }

			  protected internal virtual void Copy( InstanceAssertion instance )
			  {
					this.ServerId = instance.ServerId;
					this.Uri = instance.Uri;
					this.IsFailed = instance.IsFailed;
					this.ReachableInstances = new Dictionary<int, URI>( instance.ReachableInstances );
					this.FailedInstances = new Dictionary<int, URI>( instance.FailedInstances );
					this.Roles = new Dictionary<string, int>( instance.Roles );
			  }
		 }
	}

}