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
namespace Neo4Net.causalclustering.discovery
{

	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Implementation of the RetryStrategy that repeats the retriable function until the correct result has been retrieved or the limit of retries has been
	/// encountered.
	/// There is a fixed delay between each retry.
	/// </summary>
	/// @param <INPUT> the type of input of the retriable function </param>
	/// @param <OUTPUT> the type of output of the retriable function </param>
	public class MultiRetryStrategy<INPUT, OUTPUT> : RetryStrategy<INPUT, OUTPUT>
	{
		 private readonly long _delayInMillis;
		 private readonly long _retries;
		 private readonly LogProvider _logProvider;
		 private readonly System.Action<long> _sleeper;

		 /// <param name="delayInMillis"> number of milliseconds between each attempt at getting the desired result </param>
		 /// <param name="retries"> the number of attempts to perform before giving up </param> </param>
		 /// <param name="logProvider"> {<seealso cref= LogProvider} </seealso>
		 public MultiRetryStrategy( long delayInMillis, long retries, LogProvider logProvider, System.Action<long> sleeper )
		 {
			  this._delayInMillis = delayInMillis;
			  this._retries = retries;
			  this._logProvider = logProvider;
			  this._sleeper = sleeper;
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override OUTPUT Apply( INPUT retriableInput, System.Func<INPUT, OUTPUT> retriable, System.Predicate<OUTPUT> wasRetrySuccessful )
		 {
			  Log log = _logProvider.getLog( typeof( MultiRetryStrategy ) );
			  OUTPUT result = retriable( retriableInput );
			  int currentIteration = 0;
			  while ( !wasRetrySuccessful( result ) && currentIteration++ < _retries )
			  {
					log.Debug( "Try attempt was unsuccessful for input: %s\n", retriableInput );
					_sleeper.accept( _delayInMillis );
					result = retriable( retriableInput );
			  }
			  return result;
		 }
	}

}