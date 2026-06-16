using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.IntegrationTests;

[CollectionDefinition("Postgres collection")]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}
